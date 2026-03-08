using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.Exceptions;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Commands;
public sealed class LeaveSpaceCommandHandler : IRequestHandler<LeaveSpaceCommand, Result<LeaveSpaceResult>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly ISettlementRepository _settlementRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public LeaveSpaceCommandHandler(
        ISpaceRepository spaceRepository,
        ISettlementRepository settlementRepository,
        IUserContext userContext,
        IClock clock)
    {
        _spaceRepository = spaceRepository;
        _settlementRepository = settlementRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<LeaveSpaceResult>> Handle(LeaveSpaceCommand command, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(command.SpaceId);
        var specification = new SpaceByIdSpecification(spaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(specification, cancellationToken);

        if (space == null)
        {
            return Result<LeaveSpaceResult>.Failure(Error.SpaceNotFound(command.SpaceId));
        }

        var currentUserId = _userContext.CurrentUserId;
        var member = space.GetMember(currentUserId);

        if (member == null)
        {
            return Result<LeaveSpaceResult>.Failure(Error.MemberNotFound(currentUserId.Value, command.SpaceId));
        }

        if (member.Role == SpaceRole.Admin)
        {
            return Result<LeaveSpaceResult>.Failure(Error.InsufficientSpacePrivileges(
                "leave this space as an admin. Please transfer admin rights first or ask another admin to remove you"));
        }

        var settlements = await _settlementRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        var hasUnpaidSettlements = settlements.Any(s =>
            s.Lines.Any(l => l.UserId == currentUserId && !l.IsConfirmed));

        if (hasUnpaidSettlements)
        {
            return Result<LeaveSpaceResult>.Failure(Error.UnpaidSettlements(currentUserId.Value));
        }

        try
        {
            var spaceName = space.Name;
            space.RemoveMember(currentUserId, _clock);
            await _spaceRepository.UpdateAsync(space, cancellationToken);

            return Result<LeaveSpaceResult>.Success(new LeaveSpaceResult(command.SpaceId, spaceName));
        }
        catch (Exception ex) when (ex is SpaceDomainException || ex is InvariantViolationException)
        {
            return Result<LeaveSpaceResult>.Failure(Error.SystemFailure("leave space"));
        }
    }
}
