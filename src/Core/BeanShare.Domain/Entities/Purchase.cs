using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Entities;

public sealed class Purchase : Entity
{
    public Guid Id { get; private init; }
    public CoffeeProduct Product { get; private init; }
    public Weight Quantity { get; private init; }
    public Money Cost { get; private init; }
    public string Vendor { get; private init; }
    public UserId PurchasedBy { get; private init; }
    public DateTime PurchasedAt { get; private init; }
    public DateTime CreatedAt { get; private init; }

    private Purchase()
    {
        Product = null!;
        Quantity = null!;
        Cost = null!;
        Vendor = string.Empty;
        PurchasedBy = default;
    }

    private Purchase(
        Guid id,
        CoffeeProduct product,
        Weight quantity,
        Money cost,
        string vendor,
        UserId purchasedBy,
        DateTime purchasedAt,
        DateTime createdAt)
    {
        Id = id;
        Product = product;
        Quantity = quantity;
        Cost = cost;
        Vendor = vendor;
        PurchasedBy = purchasedBy;
        PurchasedAt = purchasedAt;
        CreatedAt = createdAt;
    }

    public static Purchase Create(
        CoffeeProduct product,
        Weight quantity,
        Money cost,
        string vendor,
        UserId purchasedBy,
        DateTime purchasedAt,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(quantity);
        ArgumentNullException.ThrowIfNull(cost);
        ArgumentNullException.ThrowIfNull(purchasedBy);

        if (string.IsNullOrWhiteSpace(vendor))
            throw new ArgumentException("Vendor is required", nameof(vendor));

        if (vendor.Length > 100)
            throw new ArgumentException("Vendor name cannot exceed 100 characters", nameof(vendor));

        if (!quantity.IsPositive)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (cost.Amount <= 0)
            throw new ArgumentException("Cost must be positive", nameof(cost));

        if (purchasedAt > clock.UtcNow)
            throw new ArgumentException("Purchase date cannot be in the future", nameof(purchasedAt));

        return new Purchase(
            Guid.NewGuid(),
            product,
            quantity,
            cost,
            vendor.Trim(),
            purchasedBy,
            purchasedAt,
            clock.UtcNow);
    }

    public Money CostPerGram => Cost.Divide(Quantity.Grams);

    protected override object GetId() => Id;

    public override string ToString() => $"{Product} - {Quantity} for {Cost} from {Vendor}";
}