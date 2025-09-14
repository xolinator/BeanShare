using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, T> : IPipelineBehavior<TRequest, Result<T>>
    where TRequest : ICommand<Result<T>>
{
    private readonly IUnitOfWork _uow;
    public UnitOfWorkBehavior(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<T>> Handle(TRequest request, RequestHandlerDelegate<Result<T>> next, CancellationToken ct)
    {
        var result = await next();
        if (result.IsSuccess)
        {
            await _uow.SaveChangesAsync(ct);
        }

        return result;
    }
}