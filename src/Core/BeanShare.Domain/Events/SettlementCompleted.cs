using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

/// <summary>
/// Domain event raised when all payments in a settlement have been confirmed
/// and the settlement is marked as completed.
/// </summary>
public sealed record SettlementCompleted(
    SettlementId SettlementId,
    SpaceId SpaceId,
    BillingPeriodId BillingPeriodId,
    DateTime CompletedAt
) : IDomainEvent
{
    public DateTime OccurredOn => CompletedAt;
}
