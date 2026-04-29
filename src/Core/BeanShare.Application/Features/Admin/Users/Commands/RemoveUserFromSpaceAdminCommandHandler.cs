using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Exceptions;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Admin.Users.Commands;

public sealed class RemoveUserFromSpaceAdminCommandHandler : IRequestHandler<RemoveUserFromSpaceAdminCommand, Result>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IClock _clock;

    public RemoveUserFromSpaceAdminCommandHandler(
        ISpaceRepository spaceRepository,
        IClock clock)
    {
        _spaceRepository = spaceRepository;
        _clock = clock;
    }

    public async Task<Result> Handle(RemoveUserFromSpaceAdminCommand request, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var userId = new UserId(request.UserId);

        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);
        if (space is null)
            return Result.Failure(Error.SpaceNotFound(request.SpaceId));

        if (!space.HasMember(userId))
            return Result.Failure(Error.MemberNotFound(request.UserId, request.SpaceId));

        try
        {
            space.RemoveMember(userId, _clock);
            await _spaceRepository.UpdateAsync(space, cancellationToken);
            return Result.Success();
        }
        catch (InvariantViolationException)
        {
            return Result.Failure(Error.CannotRemoveLastAdmin());
        }
    }
}
