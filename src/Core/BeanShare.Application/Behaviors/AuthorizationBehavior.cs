using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Behaviors;

public sealed class AuthorizationBehavior<TRequest, T> : IPipelineBehavior<TRequest, Result<T>>
    where TRequest : IRequest<Result<T>>
{
    private readonly IUserContext _user;
    public AuthorizationBehavior(IUserContext user) => _user = user;

    public async Task<Result<T>> Handle(TRequest request, RequestHandlerDelegate<Result<T>> next, CancellationToken ct)
    {
        if (_user.CurrentUserId.Equals(default))
        {
            return Result<T>.Failure(Error.Unauthorized());
        }

        return await next();
    }
}