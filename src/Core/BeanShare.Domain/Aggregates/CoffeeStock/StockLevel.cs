using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using BeanShare.Domain.Exceptions;

namespace BeanShare.Domain.Aggregates.CoffeeStock;

public sealed class StockLevel : Entity
{
    public Guid Id { get; private init; }
    public CoffeeProduct Product { get; private init; }
    public Weight TotalPurchased { get; private set; }
    public Weight TotalConsumed { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private StockLevel()
    {
        Product = null!;
        TotalPurchased = null!;
        TotalConsumed = null!;
    }

    private StockLevel(
        Guid id,
        CoffeeProduct product,
        Weight initialQuantity,
        DateTime createdAt)
    {
        Id = id;
        Product = product;
        TotalPurchased = initialQuantity;
        TotalConsumed = Weight.Zero;
        UpdatedAt = createdAt;
    }

    public static StockLevel Create(CoffeeProduct product, Weight initialQuantity, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(initialQuantity);

        if (!initialQuantity.IsPositive)
            throw new ArgumentException("Initial quantity must be positive", nameof(initialQuantity));

        return new StockLevel(Guid.NewGuid(), product, initialQuantity, clock.UtcNow);
    }

    public Weight CurrentStock => TotalPurchased.Subtract(TotalConsumed);

    public bool IsLowStock(Weight threshold) => CurrentStock <= threshold;

    public void AddPurchase(Weight quantity, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(quantity);

        if (!quantity.IsPositive)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        TotalPurchased = TotalPurchased.Add(quantity);
        UpdatedAt = clock.UtcNow;
    }

    public void ConsumeStock(Weight quantity, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(quantity);

        if (!quantity.IsPositive)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var newConsumed = TotalConsumed.Add(quantity);
        if (newConsumed > TotalPurchased)
            throw new InsufficientStockException(Product.Name, Product.Brand, quantity.Grams, CurrentStock.Grams);

        TotalConsumed = newConsumed;
        UpdatedAt = clock.UtcNow;
    }

    public void RestoreStock(Weight quantity, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(quantity);

        if (!quantity.IsPositive)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var newConsumed = TotalConsumed.Subtract(quantity);
        TotalConsumed = newConsumed;
        UpdatedAt = clock.UtcNow;
    }

    public decimal ConsumptionPercentage => TotalPurchased.IsZero ? 0 : (TotalConsumed.Grams / TotalPurchased.Grams) * 100;

    public void Archive(IClock clock)
    {
        IsArchived = true;
        UpdatedAt = clock.UtcNow;
    }

    public void Unarchive(IClock clock)
    {
        IsArchived = false;
        UpdatedAt = clock.UtcNow;
    }

    protected override object GetId() => Id;

    public override string ToString() => $"{Product}: {CurrentStock} available ({ConsumptionPercentage:F1}% consumed)";
}
