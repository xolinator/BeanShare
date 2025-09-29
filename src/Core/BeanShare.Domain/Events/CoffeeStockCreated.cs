using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record CoffeeStockCreated(
    CoffeeStockId CoffeeStockId,
    SpaceId SpaceId,
    DateTime CreatedAt
) : IDomainEvent
{
    public DateTime OccurredOn => CreatedAt;
}