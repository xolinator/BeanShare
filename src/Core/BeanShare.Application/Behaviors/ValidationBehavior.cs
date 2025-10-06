using BeanShare.Application.Common;
using FluentValidation;
using MediatR;
using System.Reflection;

namespace BeanShare.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct))))
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .ToList();

        if (failures.Any())
        {
            var errors = failures.Select(f => Error.ValidationFailure(f.PropertyName, f.ErrorMessage)).ToList();
            
            var responseType = typeof(TResponse);
            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = responseType.GetGenericArguments()[0];
                var failureMethod = typeof(Result<>).MakeGenericType(resultType)
                    .GetMethod("Failure", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(List<Error>) }, null);

                if (failureMethod != null)
                {
                    var result = failureMethod.Invoke(null, new object[] { errors });
                    return (TResponse)result!;
                }
            }
            else if (responseType == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(errors);
            }
        }

        return await next();
    }
}