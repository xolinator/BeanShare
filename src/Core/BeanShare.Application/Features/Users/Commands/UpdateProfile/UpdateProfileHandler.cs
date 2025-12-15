using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Services;
using MediatR;

namespace BeanShare.Application.Features.Users.Commands.UpdateProfile;

public sealed class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, Result>
{
    private readonly IUserService _userService;
    private readonly IUserContext _userContext;

    public UpdateProfileHandler(
        IUserService userService,
        IUserContext userContext)
    {
        _userService = userService;
        _userContext = userContext;
    }

    public async Task<Result> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.CurrentUserId;

        var user = await _userService.GetByIdForUpdateAsync(currentUserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.UserNotFound(currentUserId.Value));
        }

        user.UpdateProfile(command.Name, command.PictureUrl);
        await _userService.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
