using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUnitOfWork _uow;
    public UnitOfWorkBehavior(IUnitOfWork uow) => _uow = uow;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var response = await next();

        var isCommand = request.GetType().GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

        if (isCommand && response != null)
        {
            var responseType = response.GetType();

            bool isResult = responseType == typeof(Result);
            bool isGenericResult = responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>);

            if (isResult || isGenericResult)
            {
                var isSuccessProperty = responseType.GetProperty("IsSuccess");
                if (isSuccessProperty != null)
                {
                    var isSuccess = (bool)isSuccessProperty.GetValue(response)!;

                    if (isSuccess)
                    {
                        await _uow.SaveChangesAsync(ct);
                    }
                }
            }
        }

        return response;
    }
}