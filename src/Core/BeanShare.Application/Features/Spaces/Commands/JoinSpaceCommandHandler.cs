using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class JoinSpaceCommandHandler(
    ISpaceRepository spaceRepository,
    IUserContext userContext,
    IClock clock) : IRequestHandler<JoinSpaceCommand, Result<JoinSpaceResult>>
{
    public async Task<Result<JoinSpaceResult>> Handle(JoinSpaceCommand request, CancellationToken cancellationToken)
    {
        var inviteCode = new InviteCode(request.InviteCode);
        var space = await spaceRepository.GetSingleBySpecAsync(
            new SpaceByInviteCodeSpecification(inviteCode), cancellationToken);

        if (space == null)
            return Result<JoinSpaceResult>.Failure(Error.InviteCodeNotFound(request.InviteCode));

        if (space.HasMember(userContext.CurrentUserId))
            return Result<JoinSpaceResult>.Failure(Error.AlreadySpaceMember(space.Id.Value, userContext.CurrentUserId));

        space.Join(userContext.CurrentUserId, clock);
        await spaceRepository.UpdateAsync(space, cancellationToken);

        return Result<JoinSpaceResult>.Success(new JoinSpaceResult(space.Id.Value, space.Name));
    }
}