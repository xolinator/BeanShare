using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record SpaceDeactivated(
    SpaceId SpaceId,
    DateTime DeactivatedAt
) : IDomainEvent
{
    public DateTime OccurredOn => DeactivatedAt;
}