using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Events;

public sealed record ConsumptionRecorded(
    Guid ConsumptionEntryId,
    Guid SpaceId,
    Guid UserId,
    CoffeeProduct Product,
    Weight Quantity,
    DateTime ConsumedAt,
    DateTime OccurredOn
) : IDomainEvent;