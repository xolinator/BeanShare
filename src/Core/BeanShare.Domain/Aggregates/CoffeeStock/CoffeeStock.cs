using BeanShare.Domain.Common;
using BeanShare.Domain.Events;
using BeanShare.Domain.ValueObjects;
using BeanShare.Domain.Exceptions;

namespace BeanShare.Domain.Aggregates.CoffeeStock;

public sealed class CoffeeStock : AggregateRoot
{
    private readonly List<Purchase> _purchases = [];
    private readonly List<StockLevel> _stockLevels = [];

    public CoffeeStockId Id { get; private set; }
    public SpaceId SpaceId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<Purchase> Purchases => _purchases.AsReadOnly();
    public IReadOnlyCollection<StockLevel> StockLevels => _stockLevels.AsReadOnly();

    private CoffeeStock()
    {
        Id = null!;
        SpaceId = default;
    }

    private CoffeeStock(CoffeeStockId id, SpaceId spaceId, DateTime createdAt)
    {
        Id = id;
        SpaceId = spaceId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public static CoffeeStock Create(SpaceId spaceId, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(spaceId);

        var id = CoffeeStockId.New();
        var createdAt = clock.UtcNow;

        var stock = new CoffeeStock(id, spaceId, createdAt);

        stock.RaiseDomainEvent(new CoffeeStockCreated(
            id,
            spaceId,
            createdAt));

        return stock;
    }

    public void AddPurchase(
        CoffeeProduct product,
        Weight quantity,
        Money cost,
        string vendor,
        UserId purchasedBy,
        DateTime purchasedAt,
        IClock clock)
    {
        var purchase = Purchase.Create(product, quantity, cost, vendor, purchasedBy, purchasedAt, clock);
        _purchases.Add(purchase);

        var existingStockLevel = _stockLevels.FirstOrDefault(sl =>
            sl.Product.Name == product.Name &&
            sl.Product.Brand == product.Brand &&
            sl.Product.Type == product.Type);

        if (existingStockLevel != null)
        {
            existingStockLevel.AddPurchase(quantity, clock);
        }
        else
        {
            var newStockLevel = StockLevel.Create(product, quantity, clock);
            _stockLevels.Add(newStockLevel);
        }

        UpdatedAt = clock.UtcNow;

        RaiseDomainEvent(new StockPurchaseAdded(
            Id,
            SpaceId,
            purchase.Id,
            product,
            quantity,
            cost,
            vendor,
            purchasedBy,
            purchasedAt,
            clock.UtcNow));
    }

    public void ConsumeStock(CoffeeProduct product, Weight quantity, IClock clock)
    {
        var stockLevel = _stockLevels.FirstOrDefault(sl =>
            sl.Product.Name == product.Name &&
            sl.Product.Brand == product.Brand &&
            sl.Product.Type == product.Type);

        if (stockLevel == null)
            throw new ProductNotFoundException(product.Name, product.Brand);

        stockLevel.ConsumeStock(quantity, clock);
        UpdatedAt = clock.UtcNow;

        RaiseDomainEvent(new StockConsumed(
            Id,
            SpaceId,
            product,
            quantity,
            stockLevel.CurrentStock,
            clock.UtcNow));
    }

    public Weight GetCurrentStock(CoffeeProduct product)
    {
        var stockLevel = _stockLevels.FirstOrDefault(sl =>
            sl.Product.Name == product.Name &&
            sl.Product.Brand == product.Brand &&
            sl.Product.Type == product.Type);

        return stockLevel?.CurrentStock ?? Weight.Zero;
    }

    public Money CalculateAverageCostPerGram(CoffeeProduct product)
    {
        var productPurchases = _purchases.Where(p =>
            p.Product.Name == product.Name &&
            p.Product.Brand == product.Brand &&
            p.Product.Type == product.Type).ToList();

        if (!productPurchases.Any())
            return Money.Zero(Currency.USD);

        var currency = productPurchases.First().Cost.Currency;
        var totalAmount = 0m;
        var totalWeight = 0m;

        foreach (var purchase in productPurchases)
        {
            // Only sum amounts in the same currency to avoid mixing currencies
            if (purchase.Cost.Currency == currency)
            {
                totalAmount += purchase.Cost.Amount;
                totalWeight += purchase.Quantity.Grams;
            }
        }

        return totalWeight == 0 ? Money.Zero(currency) : Money.Create(totalAmount / totalWeight, currency);
    }

    public IEnumerable<StockLevel> GetLowStockProducts(Weight threshold)
    {
        return _stockLevels.Where(sl => sl.IsLowStock(threshold));
    }

    public Money GetTotalInvestment(string currency = "USD")
    {
        var totalAmount = _purchases
            .Where(p => p.Cost.Currency == currency)
            .Sum(p => p.Cost.Amount);

        return Money.Create(totalAmount, currency);
    }

    public bool HasStock(CoffeeProduct product)
    {
        return GetCurrentStock(product).IsPositive;
    }

    public int ProductVarietyCount => _stockLevels.Count(sl => sl.CurrentStock.IsPositive);

    public Weight TotalCurrentStock => Weight.FromGrams(
        _stockLevels.Sum(sl => sl.CurrentStock.Grams));
}