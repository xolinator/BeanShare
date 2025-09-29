using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record StockPurchaseAdded(
    CoffeeStockId CoffeeStockId,
    SpaceId SpaceId,
    Guid PurchaseId,
    CoffeeProduct Product,
    Weight Quantity,
    Money Cost,
    string Vendor,
    UserId PurchasedBy,
    DateTime PurchasedAt,
    DateTime OccurredOn
) : IDomainEvent;