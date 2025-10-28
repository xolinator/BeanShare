using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record SettlementGenerated(
    SettlementId SettlementId,
    SpaceId SpaceId,
    BillingPeriodId BillingPeriodId,
    UserId GeneratedBy,
    DateTime GeneratedAt
) : IDomainEvent
{
    public DateTime OccurredOn => GeneratedAt;
}

public sealed record SettlementLineAdded(
    SettlementId SettlementId,
    UserId UserId,
    decimal AmountDue,
    string Currency,
    DateTime AddedAt
) : IDomainEvent
{
    public DateTime OccurredOn => AddedAt;
}