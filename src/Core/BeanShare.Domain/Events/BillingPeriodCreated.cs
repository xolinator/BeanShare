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
