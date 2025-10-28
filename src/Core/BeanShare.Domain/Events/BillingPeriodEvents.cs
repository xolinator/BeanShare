using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record BillingPeriodCreated(
    BillingPeriodId BillingPeriodId,
    SpaceId SpaceId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    UserId CreatedBy,
    DateTime CreatedAt
) : IDomainEvent
{
    public DateTime OccurredOn => CreatedAt;
}

public sealed record BillingPeriodOpened(
    BillingPeriodId BillingPeriodId,
    SpaceId SpaceId,
    UserId OpenedBy,
    DateTime OpenedAt
) : IDomainEvent
{
    public DateTime OccurredOn => OpenedAt;
}

public sealed record BillingPeriodClosed(
    BillingPeriodId BillingPeriodId,
    SpaceId SpaceId,
    UserId ClosedBy,
    DateTime ClosedAt
) : IDomainEvent
{
    public DateTime OccurredOn => ClosedAt;
}

public sealed record BillingPeriodSettled(
    BillingPeriodId BillingPeriodId,
    SpaceId SpaceId,
    UserId SettledBy,
    DateTime SettledAt
) : IDomainEvent
{
    public DateTime OccurredOn => SettledAt;
}