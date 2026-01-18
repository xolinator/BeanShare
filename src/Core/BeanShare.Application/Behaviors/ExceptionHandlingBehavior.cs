using BeanShare.Application.Common;
using BeanShare.Domain.Exceptions;
using MediatR;

namespace BeanShare.Application.Behaviors;

public sealed class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (SpaceDomainException ex)
        {
            return CreateFailureResult<TResponse>(Error.DomainError(ex.Message));
        }
        catch (DomainException ex)
        {
            return CreateFailureResult<TResponse>(Error.DomainError(ex.Message));
        }
    }

    private static TResponse CreateFailureResult<T>(Error error)
    {
        var responseType = typeof(T);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result<>).MakeGenericType(resultType)
                .GetMethod("Failure", new[] { typeof(Error) });

            if (failureMethod != null)
            {
                var result = failureMethod.Invoke(null, new object[] { error });
                return (TResponse)result!;
            }
        }

        throw new InvalidOperationException($"Cannot create failure result for type {responseType}");
    }
}