using System.Collections.Concurrent;
using System.Reflection;
using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Common.Authorization;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using BeanShare.Domain.Specifications;
using MediatR;

namespace BeanShare.Application.Pipeline;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IRequest<TResponse>
{
    private static readonly ConcurrentDictionary<Type, Func<object, SpaceId?>> SpaceIdAccessors = new();

    private readonly IUserContext _userContext;
    private readonly ISpaceRepository _spaceRepository;

    public AuthorizationBehavior(IUserContext userContext, ISpaceRepository spaceRepository)
    {
        _userContext = userContext;
        _spaceRepository = spaceRepository;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IAuthorize)
        {
            return await next();
        }

        var requirements = ExtractRequirements(request);

        if (requirements.Count == 0)
        {
            var authResult = await EnsureAuthenticatedAsync();
            if (authResult.IsFailure)
            {
                return CreateFailureResponse<TResponse>(authResult.Errors);
            }
            return await next();
        }

        foreach (var requirement in requirements)
        {
            var result = requirement switch
            {
                RequireSpaceMemberAttribute spaceMember => await ValidateSpaceMemberAsync(request, spaceMember, cancellationToken),
                RequireSpaceAdminAttribute spaceAdmin => await ValidateSpaceAdminAsync(request, spaceAdmin, cancellationToken),
                _ => Result.Success()
            };

            if (result.IsFailure)
            {
                return CreateFailureResponse<TResponse>(result.Errors);
            }
        }

        return await next();
    }

    private static List<object> ExtractRequirements(TRequest request)
    {
        var requirements = new List<object>();
        var requestType = request.GetType();

        var attributes = requestType.GetCustomAttributes(inherit: true);

        foreach (var attribute in attributes)
        {
            if (attribute is RequireSpaceMemberAttribute spaceMember)
            {
                requirements.Add(spaceMember);
            }
            else if (attribute is RequireSpaceAdminAttribute spaceAdmin)
            {
                requirements.Add(spaceAdmin);
            }
        }

        return requirements;
    }

    private Task<Result> EnsureAuthenticatedAsync()
    {
        try
        {
            _ = _userContext.CurrentUserId;
            return Task.FromResult(Result.Success());
        }
        catch (InvalidOperationException)
        {
            return Task.FromResult(Result.Failure(new Error("auth.unauthenticated", "User is not authenticated")));
        }
    }

    private async Task<Result> ValidateSpaceMemberAsync(TRequest request, RequireSpaceMemberAttribute requirement, CancellationToken cancellationToken)
    {
        var authResult = await EnsureAuthenticatedAsync();
        if (authResult.IsFailure)
        {
            return authResult;
        }

        var spaceId = ExtractSpaceId(request, requirement.SpaceIdPropertyName);
        if (spaceId == null)
        {
            return Result.Failure(new Error("auth.invalid_request", $"Property '{requirement.SpaceIdPropertyName}' not found or invalid"));
        }

        var specification = new SpaceByIdSpecification(spaceId.Value);
        var space = await _spaceRepository.GetSingleBySpecAsync(specification, cancellationToken);
        if (space == null)
        {
            return Result.Failure(new Error("auth.space_not_found", "Space not found"));
        }

        var userId = _userContext.CurrentUserId;
        if (!space.HasMember(userId))
        {
            return Result.Failure(new Error("auth.not_member", "User is not a member of this space"));
        }

        return Result.Success();
    }

    private async Task<Result> ValidateSpaceAdminAsync(TRequest request, RequireSpaceAdminAttribute requirement, CancellationToken cancellationToken)
    {
        var authResult = await EnsureAuthenticatedAsync();
        if (authResult.IsFailure)
        {
            return authResult;
        }

        var spaceId = ExtractSpaceId(request, requirement.SpaceIdPropertyName);
        if (spaceId == null)
        {
            return Result.Failure(new Error("auth.invalid_request", $"Property '{requirement.SpaceIdPropertyName}' not found or invalid"));
        }

        var specification = new SpaceByIdSpecification(spaceId.Value);
        var space = await _spaceRepository.GetSingleBySpecAsync(specification, cancellationToken);
        if (space == null)
        {
            return Result.Failure(new Error("auth.space_not_found", "Space not found"));
        }

        var userId = _userContext.CurrentUserId;
        if (!space.IsAdmin(userId))
        {
            return Result.Failure(new Error("auth.not_admin", "User is not an admin of this space"));
        }

        return Result.Success();
    }

    private static SpaceId? ExtractSpaceId(TRequest request, string propertyName)
    {
        var requestType = request.GetType();

        var accessor = SpaceIdAccessors.GetOrAdd(requestType, type =>
        {
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
            {
                return (Func<object, SpaceId?>)(_ => null);
            }

            return (Func<object, SpaceId?>)(obj =>
            {
                var value = property.GetValue(obj);

                if (property.PropertyType == typeof(Guid?) && value is Guid nullableGuid)
                {
                    return new SpaceId(nullableGuid);
                }

                return value switch
                {
                    SpaceId spaceId => spaceId,
                    Guid guid => new SpaceId(guid),
                    string str when Guid.TryParse(str, out var guid) => new SpaceId(guid),
                    _ => null
                };
            });
        });

        return accessor(request);
    }

    private static TResponse CreateFailureResponse<T>(IReadOnlyList<Error> errors)
    {
        var responseType = typeof(T);

        try
        {
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var valueType = responseType.GetGenericArguments()[0];
                var failureMethod = typeof(Result<>).MakeGenericType(valueType)
                    .GetMethod("Failure", new[] { typeof(IReadOnlyList<Error>) });

                if (failureMethod == null)
                {
                    throw new InvalidOperationException($"Could not find Failure method on Result<{valueType.Name}>");
                }

                var result = failureMethod.Invoke(null, new object[] { errors });
                if (result == null)
                {
                    throw new InvalidOperationException($"Failure method returned null for Result<{valueType.Name}>");
                }

                return (TResponse)result;
            }

            if (responseType == typeof(Result))
            {
                var failureMethod = typeof(Result).GetMethod("Failure", new[] { typeof(IReadOnlyList<Error>) });
                if (failureMethod == null)
                {
                    throw new InvalidOperationException("Could not find Failure method on Result");
                }

                var result = failureMethod.Invoke(null, new object[] { errors });
                if (result == null)
                {
                    throw new InvalidOperationException("Failure method returned null for Result");
                }

                return (TResponse)result;
            }

            throw new InvalidOperationException($"Response type {responseType.Name} is not a supported Result type. Authorization behavior should only be used with Result or Result<T> types.");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to create failure response for type {responseType.Name}: {ex.Message}", ex);
        }
    }
}