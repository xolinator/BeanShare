using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record BillingPeriodClosed(
    BillingPeriodId BillingPeriodId,
    SpaceId SpaceId,
    UserId ClosedBy,
    DateTime ClosedAt
) : IDomainEvent
{
    public DateTime OccurredOn => ClosedAt;
}
