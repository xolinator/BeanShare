using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Settlement.Commands.ConfirmPayment;

public sealed class ConfirmPaymentHandler(
    ISettlementRepository settlementRepository,
    ISpaceRepository spaceRepository,
    IUserContext userContext,
    IClock clock) : IRequestHandler<ConfirmPaymentCommand, Result>
{
    public async Task<Result> Handle(ConfirmPaymentCommand command, CancellationToken cancellationToken)
    {
        var settlement = await settlementRepository.GetByIdAsync(
            new SettlementId(command.SettlementId), cancellationToken);

        if (settlement is null)
            return Result.Failure(Error.SettlementNotFound(command.SettlementId));

        var space = await spaceRepository.GetSingleBySpecAsync(
            new SpaceByIdSpecification(settlement.SpaceId), cancellationToken);

        if (space is null)
            return Result.Failure(Error.SpaceNotFound(settlement.SpaceId.Value));

        if (!CanConfirmPayment(space, command.MemberUserId))
            return Result.Failure(Error.InsufficientSpacePrivileges("confirm payments for other members"));

        var memberUserId = new UserId(command.MemberUserId);
        var line = settlement.GetLineForUser(memberUserId);

        if (line is null)
            return Result.Failure(Error.SettlementLineNotFound(command.MemberUserId));
        if (line.IsConfirmed)
            return Result.Failure(Error.PaymentAlreadyConfirmed(command.MemberUserId));
        if (settlement.Status is SettlementStatus.Completed or SettlementStatus.Generated)
            return Result.Failure(Error.InvalidSettlementState("confirm payment", settlement.Status.ToString()));

        try
        {
            settlement.ConfirmPayment(memberUserId, userContext.CurrentUserId, clock);
            await settlementRepository.UpdateAsync(settlement, cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.ConfirmationFailed(ex.Message));
        }
    }

    private bool CanConfirmPayment(Domain.Aggregates.Space.Space space, Guid targetMemberUserId)
    {
        var currentUserId = userContext.CurrentUserId;
        return space.IsAdmin(currentUserId) || currentUserId.Value == targetMemberUserId;
    }
}
