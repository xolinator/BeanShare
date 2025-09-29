using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record StockConsumed(
    CoffeeStockId CoffeeStockId,
    SpaceId SpaceId,
    CoffeeProduct Product,
    Weight ConsumedQuantity,
    Weight RemainingStock,
    DateTime OccurredOn
) : IDomainEvent;