using BeanShare.Domain.Common;
using BeanShare.Domain.Events;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Entities;

public sealed class ConsumptionEntry : AggregateRoot
{
    public ConsumptionEntryId Id { get; private set; }
    public SpaceId SpaceId { get; private set; }
    public UserId UserId { get; private set; }
    public CoffeeProduct Product { get; private set; }
    public Weight Quantity { get; private set; }
    public DateTime ConsumedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public BillingPeriodId? BillingPeriodId { get; private set; }
    public PresetRecipeId? PresetId { get; private set; }
    public string? PresetName { get; private set; }

    private ConsumptionEntry()
    {
        Id = default!;
        SpaceId = default;
        UserId = default;
        Product = CoffeeProduct.Create("Unknown", "Unknown", CoffeeType.Espresso);
        Quantity = default!;
    }

    private ConsumptionEntry(
        ConsumptionEntryId id,
        SpaceId spaceId,
        UserId userId,
        CoffeeProduct product,
        Weight quantity,
        DateTime consumedAt,
        DateTime createdAt,
        PresetRecipeId? presetId = null,
        string? presetName = null)
    {
        Id = id;
        SpaceId = spaceId;
        UserId = userId;
        Product = product;
        Quantity = quantity;
        ConsumedAt = consumedAt;
        CreatedAt = createdAt;
        PresetId = presetId;
        PresetName = presetName;
    }

    public static ConsumptionEntry Create(
        SpaceId spaceId,
        UserId userId,
        CoffeeProduct product,
        Weight quantity,
        DateTime consumedAt,
        IClock clock,
        PresetRecipeId? presetId = null,
        string? presetName = null)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(clock);

        if (quantity.IsZero || !quantity.IsPositive)
        {
            throw new ArgumentException("Consumption quantity must be positive", nameof(quantity));
        }

        // Allow a small tolerance of 1 minute for timezone conversion and clock drift
        if (consumedAt > clock.UtcNow.AddMinutes(1))
        {
            throw new ArgumentException("Consumption time cannot be in the future", nameof(consumedAt));
        }

        var id = ConsumptionEntryId.New();
        var createdAt = clock.UtcNow;

        var entry = new ConsumptionEntry(id, spaceId, userId, product, quantity, consumedAt, createdAt, presetId, presetName);

        entry.RaiseDomainEvent(new ConsumptionRecorded(
            entry.Id.Value,
            entry.SpaceId.Value,
            entry.UserId.Value,
            entry.Product,
            entry.Quantity,
            entry.ConsumedAt,
            clock.UtcNow));

        return entry;
    }

    public void AssignToBillingPeriod(BillingPeriodId billingPeriodId)
    {
        ArgumentNullException.ThrowIfNull(billingPeriodId);

        if (BillingPeriodId != null)
        {
            throw new InvalidOperationException("Consumption is already assigned to a billing period");
        }

        BillingPeriodId = billingPeriodId;
    }
}