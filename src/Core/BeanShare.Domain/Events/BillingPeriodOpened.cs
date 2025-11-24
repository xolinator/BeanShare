using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record BillingPeriodOpened(
    BillingPeriodId BillingPeriodId,
    SpaceId SpaceId,
    UserId OpenedBy,
    DateTime OpenedAt
) : IDomainEvent
{
    public DateTime OccurredOn => OpenedAt;
}
