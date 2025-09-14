using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class JoinSpaceCommandHandler : IRequestHandler<JoinSpaceCommand, Result<JoinSpaceResult>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public JoinSpaceCommandHandler(
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IClock clock)
    {
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<JoinSpaceResult>> Handle(JoinSpaceCommand request, CancellationToken cancellationToken)
    {
        var inviteCode = new InviteCode(request.InviteCode);
        var space = await _spaceRepository.GetByInviteCodeAsync(inviteCode, cancellationToken);

        if (space == null)
        {
            return Result<JoinSpaceResult>.Failure(Error.InviteCodeNotFound(request.InviteCode));
        }

        if (space.HasMember(_userContext.CurrentUserId))
        {
            return Result<JoinSpaceResult>.Success(new JoinSpaceResult(space.Id.Value, space.Name));
        }

        space.Join(_userContext.CurrentUserId, _clock);
        await _spaceRepository.UpdateAsync(space, cancellationToken);

        return Result<JoinSpaceResult>.Success(new JoinSpaceResult(space.Id.Value, space.Name));
    }
}