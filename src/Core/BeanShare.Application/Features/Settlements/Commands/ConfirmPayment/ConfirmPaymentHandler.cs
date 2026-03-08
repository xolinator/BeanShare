using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Settlements.Commands.ConfirmPayment;
public sealed class ConfirmPaymentHandler : IRequestHandler<ConfirmPaymentCommand, Result>
{
    private readonly ISettlementRepository _settlementRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public ConfirmPaymentHandler(
        ISettlementRepository settlementRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IClock clock)
    {
        _settlementRepository = settlementRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result> Handle(ConfirmPaymentCommand command, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.CurrentUserId;
        var settlementId = new SettlementId(command.SettlementId);
        var memberUserId = new UserId(command.MemberUserId);

        var settlement = await _settlementRepository.GetByIdAsync(settlementId, cancellationToken);
        if (settlement is null)
        {
            return Result.Failure(Error.SettlementNotFound(command.SettlementId));
        }

        var spaceSpec = new SpaceByIdSpecification(settlement.SpaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);

        if (space is null)
        {
            return Result.Failure(Error.SpaceNotFound(settlement.SpaceId.Value));
        }

        var isAdmin = space.IsAdmin(currentUserId);
        var isSelf = currentUserId.Value == command.MemberUserId;

        if (!isAdmin && !isSelf)
        {
            return Result.Failure(Error.InsufficientSpacePrivileges("confirm payments for other members"));
        }

        var line = settlement.GetLineForUser(memberUserId);
        if (line is null)
        {
            return Result.Failure(Error.SettlementLineNotFound(command.MemberUserId));
        }

        if (line.IsConfirmed)
        {
            return Result.Failure(Error.PaymentAlreadyConfirmed(command.MemberUserId));
        }

        if (settlement.Status == SettlementStatus.Completed)
        {
            return Result.Failure(Error.InvalidSettlementState("confirm payment", settlement.Status.ToString()));
        }

        if (settlement.Status == SettlementStatus.Generated)
        {
            return Result.Failure(Error.InvalidSettlementState("confirm payment", settlement.Status.ToString()));
        }

        try
        {
            settlement.ConfirmPayment(memberUserId, currentUserId, _clock);
            await _settlementRepository.UpdateAsync(settlement, cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.ConfirmationFailed(ex.Message));
        }
    }
}
