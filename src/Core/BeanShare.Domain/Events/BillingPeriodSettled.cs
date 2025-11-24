using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record BillingPeriodSettled(
    BillingPeriodId BillingPeriodId,
    SpaceId SpaceId,
    UserId SettledBy,
    DateTime SettledAt
) : IDomainEvent
{
    public DateTime OccurredOn => SettledAt;
}
