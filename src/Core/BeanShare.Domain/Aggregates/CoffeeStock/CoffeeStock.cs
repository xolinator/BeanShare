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
            string.Equals(sl.Product.Name, product.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(sl.Product.Brand, product.Brand, StringComparison.OrdinalIgnoreCase) &&
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

    public void UpdatePurchase(Guid purchaseId, Weight newQuantity, decimal newCostAmount, DateTime newPurchasedAt, IClock clock,
        string? newProductName = null, string? newProductBrand = null, CoffeeType? newCoffeeType = null)
    {
        var purchase = _purchases.FirstOrDefault(p => p.Id == purchaseId);
        if (purchase == null)
            throw new InvalidOperationException($"Purchase {purchaseId} not found");

        var oldProduct = purchase.Product;
        var oldQuantity = purchase.Quantity;
        purchase.Update(newQuantity, newCostAmount, newPurchasedAt, clock);

        if (IsProductIdentityChanged(oldProduct, newProductName, newProductBrand, newCoffeeType))
        {
            var updatedProduct = CoffeeProduct.Create(
                newProductName ?? oldProduct.Name,
                newProductBrand ?? oldProduct.Brand,
                newCoffeeType ?? oldProduct.Type);

            purchase.UpdateProduct(updatedProduct);

            var oldStockLevel = _stockLevels.FirstOrDefault(sl =>
                string.Equals(sl.Product.Name, oldProduct.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(sl.Product.Brand, oldProduct.Brand, StringComparison.OrdinalIgnoreCase) &&
                sl.Product.Type == oldProduct.Type);

            if (oldStockLevel != null)
            {
                var maxReduction = oldStockLevel.TotalPurchased.Grams - oldStockLevel.TotalConsumed.Grams;
                var reduction = Math.Min(oldQuantity.Grams, maxReduction);
                if (reduction > 0)
                    oldStockLevel.ReducePurchased(Weight.FromGrams(reduction), clock);
            }

            var newStockLevel = _stockLevels.FirstOrDefault(sl =>
                string.Equals(sl.Product.Name, updatedProduct.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(sl.Product.Brand, updatedProduct.Brand, StringComparison.OrdinalIgnoreCase) &&
                sl.Product.Type == updatedProduct.Type);

            if (newStockLevel != null)
                newStockLevel.AddPurchase(newQuantity, clock);
            else
                _stockLevels.Add(StockLevel.Create(updatedProduct, newQuantity, clock));
        }
        else
        {
            var stockLevel = _stockLevels.FirstOrDefault(sl =>
                string.Equals(sl.Product.Name, purchase.Product.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(sl.Product.Brand, purchase.Product.Brand, StringComparison.OrdinalIgnoreCase) &&
                sl.Product.Type == purchase.Product.Type);

            if (stockLevel != null)
            {
                var diff = newQuantity.Grams - oldQuantity.Grams;
                if (diff > 0)
                    stockLevel.AddPurchase(Weight.FromGrams(diff), clock);
                else if (diff < 0)
                {
                    var maxReduction = stockLevel.TotalPurchased.Grams - stockLevel.TotalConsumed.Grams;
                    var reduction = Math.Min(-diff, maxReduction);
                    if (reduction > 0)
                        stockLevel.ReducePurchased(Weight.FromGrams(reduction), clock);
                }
            }
        }

        UpdatedAt = clock.UtcNow;
    }

    public void DeletePurchase(Guid purchaseId, IClock clock)
    {
        var purchase = _purchases.FirstOrDefault(p => p.Id == purchaseId);
        if (purchase == null)
            throw new InvalidOperationException($"Purchase {purchaseId} not found");

        var stockLevel = _stockLevels.FirstOrDefault(sl =>
            string.Equals(sl.Product.Name, purchase.Product.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(sl.Product.Brand, purchase.Product.Brand, StringComparison.OrdinalIgnoreCase) &&
            sl.Product.Type == purchase.Product.Type);

        if (stockLevel != null)
        {
            var maxReduction = stockLevel.TotalPurchased.Grams - stockLevel.TotalConsumed.Grams;
            var reduction = Math.Min(purchase.Quantity.Grams, maxReduction);
            if (reduction > 0)
                stockLevel.ReducePurchased(Weight.FromGrams(reduction), clock);
        }

        _purchases.Remove(purchase);
        UpdatedAt = clock.UtcNow;
    }

    public void ConsumeStock(CoffeeProduct product, Weight quantity, IClock clock)
    {
        var stockLevel = _stockLevels.FirstOrDefault(sl =>
            string.Equals(sl.Product.Name, product.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(sl.Product.Brand, product.Brand, StringComparison.OrdinalIgnoreCase) &&
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

    public void RestoreStock(CoffeeProduct product, Weight quantity, IClock clock)
    {
        var stockLevel = _stockLevels.FirstOrDefault(sl =>
            string.Equals(sl.Product.Name, product.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(sl.Product.Brand, product.Brand, StringComparison.OrdinalIgnoreCase) &&
            sl.Product.Type == product.Type);

        if (stockLevel == null)
            throw new ProductNotFoundException(product.Name, product.Brand);

        stockLevel.RestoreStock(quantity, clock);
        UpdatedAt = clock.UtcNow;
    }

    public Weight GetCurrentStock(CoffeeProduct product)
    {
        var stockLevel = _stockLevels.FirstOrDefault(sl =>
            string.Equals(sl.Product.Name, product.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(sl.Product.Brand, product.Brand, StringComparison.OrdinalIgnoreCase) &&
            sl.Product.Type == product.Type);

        return stockLevel?.CurrentStock ?? Weight.Zero;
    }

    public Money CalculateAverageCostPerGram(CoffeeProduct product)
    {
        var productPurchases = _purchases.Where(p =>
            string.Equals(p.Product.Name, product.Name, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.Product.Brand, product.Brand, StringComparison.OrdinalIgnoreCase) &&
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

    public void ArchiveStockLevel(Guid stockLevelId, IClock clock)
    {
        var stockLevel = _stockLevels.FirstOrDefault(sl => sl.Id == stockLevelId);
        if (stockLevel == null)
            throw new ArgumentException($"Stock level {stockLevelId} not found", nameof(stockLevelId));

        stockLevel.Archive(clock);
        UpdatedAt = clock.UtcNow;
    }

    public void UnarchiveStockLevel(Guid stockLevelId, IClock clock)
    {
        var stockLevel = _stockLevels.FirstOrDefault(sl => sl.Id == stockLevelId);
        if (stockLevel == null)
            throw new ArgumentException($"Stock level {stockLevelId} not found", nameof(stockLevelId));

        stockLevel.Unarchive(clock);
        UpdatedAt = clock.UtcNow;
    }

    public void SetCurrentlyUsed(Guid stockLevelId, IClock clock)
    {
        var target = _stockLevels.FirstOrDefault(sl => sl.Id == stockLevelId);
        if (target == null)
            throw new ArgumentException($"Stock level {stockLevelId} not found", nameof(stockLevelId));

        if (target.IsArchived)
            throw new InvalidOperationException($"Cannot set an archived stock level as currently used");

        foreach (var sl in _stockLevels.Where(sl => sl.IsCurrentlyUsed && sl.Id != stockLevelId))
            sl.ClearCurrentlyUsed(clock);

        target.SetCurrentlyUsed(clock);
        UpdatedAt = clock.UtcNow;
    }

    public void ClearCurrentlyUsed(IClock clock)
    {
        foreach (var sl in _stockLevels.Where(sl => sl.IsCurrentlyUsed))
            sl.ClearCurrentlyUsed(clock);

        UpdatedAt = clock.UtcNow;
    }

    public int ProductVarietyCount => _stockLevels.Count(sl => sl.CurrentStock.IsPositive && !sl.IsArchived);

    public Weight TotalCurrentStock => Weight.FromGrams(
        _stockLevels.Where(sl => !sl.IsArchived).Sum(sl => sl.CurrentStock.Grams));

    private static bool IsProductIdentityChanged(
        CoffeeProduct oldProduct, string? newProductName, string? newProductBrand, CoffeeType? newCoffeeType)
    {
        return (newProductName != null && !string.Equals(newProductName, oldProduct.Name, StringComparison.OrdinalIgnoreCase))
            || (newProductBrand != null && !string.Equals(newProductBrand, oldProduct.Brand, StringComparison.OrdinalIgnoreCase))
            || (newCoffeeType.HasValue && newCoffeeType.Value != oldProduct.Type);
    }
}