using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using MediatR;

namespace BeanShare.Application.Features.Admin.Users.Commands;
public sealed class SetUserRoleCommandHandler : IRequestHandler<SetUserRoleCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public SetUserRoleCommandHandler(
        IUserRepository userRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SetUserRoleCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);

        if (userId == _userContext.CurrentUserId && request.Role != SystemRole.SystemAdmin)
        {
            return Result.Failure(Error.ValidationFailure("UserId", "Cannot demote your own account"));
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "User not found"));
        }

        if (user.SystemRole == request.Role)
        {
            return Result.Success();
        }

        user.SetSystemRole(request.Role);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
