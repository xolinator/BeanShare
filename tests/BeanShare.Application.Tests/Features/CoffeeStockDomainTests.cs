using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.Common;
using BeanShare.Domain.Events;
using BeanShare.Domain.Exceptions;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Tests.Features;

public sealed class CoffeeStockDomainTests
{
    private readonly IClock _clock;
    private readonly SpaceId _spaceId;
    private readonly UserId _userId;

    public CoffeeStockDomainTests()
    {
        _clock = TestClock.Instance;
        _spaceId = new SpaceId(Guid.NewGuid());
        _userId = new UserId(Guid.NewGuid());
    }

    [Fact]
    public void CoffeeStock_Create_ShouldCreateWithCorrectProperties()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);

        coffeeStock.Should().NotBeNull();
        coffeeStock.Id.Should().NotBeNull();
        coffeeStock.SpaceId.Should().Be(_spaceId);
        coffeeStock.CreatedAt.Should().Be(_clock.UtcNow);
        coffeeStock.UpdatedAt.Should().Be(_clock.UtcNow);
        coffeeStock.Purchases.Should().BeEmpty();
        coffeeStock.StockLevels.Should().BeEmpty();
    }

    [Fact]
    public void CoffeeStock_Create_ShouldRaiseDomainEvent()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);

        var domainEvent = coffeeStock.DomainEvents.OfType<CoffeeStockCreated>().FirstOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent?.CoffeeStockId.Should().Be(coffeeStock.Id);
        domainEvent?.SpaceId.Should().Be(_spaceId);
        domainEvent?.OccurredOn.Should().Be(_clock.UtcNow);
    }

    [Fact]
    public void AddPurchase_WithValidData_ShouldAddPurchaseAndCreateStockLevel()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var quantity = Weight.FromGrams(1000);
        var cost = Money.Create(25.99m, "USD");
        var vendor = "Coffee Supplier Inc";
        var purchasedAt = _clock.UtcNow.AddDays(-1);

        coffeeStock.AddPurchase(product, quantity, cost, vendor, _userId, purchasedAt, _clock);

        coffeeStock.Purchases.Should().HaveCount(1);
        coffeeStock.StockLevels.Should().HaveCount(1);

        var purchase = coffeeStock.Purchases.First();
        purchase.Product.Should().Be(product);
        purchase.Quantity.Should().Be(quantity);
        purchase.Cost.Should().Be(cost);
        purchase.Vendor.Should().Be(vendor);
        purchase.PurchasedBy.Should().Be(_userId);
        purchase.PurchasedAt.Should().Be(purchasedAt);

        var stockLevel = coffeeStock.StockLevels.First();
        stockLevel.Product.Should().Be(product);
        stockLevel.TotalPurchased.Should().Be(quantity);
        stockLevel.TotalConsumed.Should().Be(Weight.Zero);
        stockLevel.CurrentStock.Should().Be(quantity);
    }

    [Fact]
    public void AddPurchase_WithSameProduct_ShouldUpdateExistingStockLevel()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var firstQuantity = Weight.FromGrams(1000);
        var secondQuantity = Weight.FromGrams(500);
        var cost = Money.Create(25.99m, "USD");

        coffeeStock.AddPurchase(product, firstQuantity, cost, "Vendor1", _userId, _clock.UtcNow.AddDays(-2), _clock);
        coffeeStock.AddPurchase(product, secondQuantity, cost, "Vendor2", _userId, _clock.UtcNow.AddDays(-1), _clock);

        coffeeStock.Purchases.Should().HaveCount(2);
        coffeeStock.StockLevels.Should().HaveCount(1);

        var stockLevel = coffeeStock.StockLevels.First();
        stockLevel.TotalPurchased.Should().Be(Weight.FromGrams(1500));
        stockLevel.CurrentStock.Should().Be(Weight.FromGrams(1500));
    }

    [Fact]
    public void AddPurchase_WithFutureDate_ShouldThrowException()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var quantity = Weight.FromGrams(1000);
        var cost = Money.Create(25.99m, "USD");
        var futureDate = _clock.UtcNow.AddDays(1);

        var act = () => coffeeStock.AddPurchase(product, quantity, cost, "Vendor", _userId, futureDate, _clock);
        act.Should().Throw<ArgumentException>()
           .WithMessage("*cannot be in the future*");
    }

    [Fact]
    public void AddPurchase_WithNullProduct_ShouldThrowException()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var quantity = Weight.FromGrams(1000);
        var cost = Money.Create(25.99m, "USD");

        var act = () => coffeeStock.AddPurchase(null!, quantity, cost, "Vendor", _userId, _clock.UtcNow.AddDays(-1), _clock);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddPurchase_ShouldRaiseDomainEvent()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var quantity = Weight.FromGrams(1000);
        var cost = Money.Create(25.99m, "USD");

        // Clear creation event
        coffeeStock.ClearDomainEvents();

        coffeeStock.AddPurchase(product, quantity, cost, "Vendor", _userId, _clock.UtcNow.AddDays(-1), _clock);

        var domainEvent = coffeeStock.DomainEvents.OfType<StockPurchaseAdded>().FirstOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent?.CoffeeStockId.Should().Be(coffeeStock.Id);
        domainEvent?.Product.Should().Be(product);
        domainEvent?.Quantity.Should().Be(quantity);
        domainEvent?.Cost.Should().Be(cost);
    }

    [Fact]
    public void ConsumeStock_WithValidData_ShouldReduceStockLevel()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var purchaseQuantity = Weight.FromGrams(1000);
        var consumeQuantity = Weight.FromGrams(300);

        coffeeStock.AddPurchase(product, purchaseQuantity, Money.Create(25.99m, "USD"), "Vendor", _userId, _clock.UtcNow.AddDays(-1), _clock);

        coffeeStock.ConsumeStock(product, consumeQuantity, _clock);

        var stockLevel = coffeeStock.StockLevels.First();
        stockLevel.TotalConsumed.Should().Be(consumeQuantity);
        stockLevel.CurrentStock.Should().Be(Weight.FromGrams(700));
    }

    [Fact]
    public void ConsumeStock_MoreThanAvailable_ShouldThrowException()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var purchaseQuantity = Weight.FromGrams(500);
        var consumeQuantity = Weight.FromGrams(600);

        coffeeStock.AddPurchase(product, purchaseQuantity, Money.Create(25.99m, "USD"), "Vendor", _userId, _clock.UtcNow.AddDays(-1), _clock);

        var act = () => coffeeStock.ConsumeStock(product, consumeQuantity, _clock);
        act.Should().Throw<InsufficientStockException>()
           .WithMessage("*Only*available*");
    }

    [Fact]
    public void ConsumeStock_ProductNotExists_ShouldThrowException()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var consumeQuantity = Weight.FromGrams(100);

        var act = () => coffeeStock.ConsumeStock(product, consumeQuantity, _clock);
        act.Should().Throw<ProductNotFoundException>()
           .WithMessage("*not found in stock*");
    }

    [Fact]
    public void ConsumeStock_ShouldRaiseDomainEvent()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var purchaseQuantity = Weight.FromGrams(1000);
        var consumeQuantity = Weight.FromGrams(300);

        coffeeStock.AddPurchase(product, purchaseQuantity, Money.Create(25.99m, "USD"), "Vendor", _userId, _clock.UtcNow.AddDays(-1), _clock);
        coffeeStock.ClearDomainEvents();

        coffeeStock.ConsumeStock(product, consumeQuantity, _clock);

        var domainEvent = coffeeStock.DomainEvents.OfType<StockConsumed>().FirstOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent?.CoffeeStockId.Should().Be(coffeeStock.Id);
        domainEvent?.Product.Should().Be(product);
        domainEvent?.ConsumedQuantity.Should().Be(consumeQuantity);
    }

    [Fact]
    public void CalculateAverageCostPerGram_WithSinglePurchase_ShouldReturnCorrectCost()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var quantity = Weight.FromGrams(1000);
        var cost = Money.Create(20.00m, "USD");

        coffeeStock.AddPurchase(product, quantity, cost, "Vendor", _userId, _clock.UtcNow.AddDays(-1), _clock);

        var averageCost = coffeeStock.CalculateAverageCostPerGram(product);

        averageCost.Amount.Should().Be(0.02m);
        averageCost.Currency.Should().Be("USD");
    }

    [Fact]
    public void CalculateAverageCostPerGram_WithMultiplePurchases_ShouldReturnWeightedAverage()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);

        coffeeStock.AddPurchase(product, Weight.FromGrams(1000), Money.Create(30.00m, "USD"), "Vendor1", _userId, _clock.UtcNow.AddDays(-2), _clock);

        coffeeStock.AddPurchase(product, Weight.FromGrams(500), Money.Create(10.00m, "USD"), "Vendor2", _userId, _clock.UtcNow.AddDays(-1), _clock);

        var averageCost = coffeeStock.CalculateAverageCostPerGram(product);

        averageCost.Amount.Should().Be(0.03m);
        averageCost.Currency.Should().Be("USD");
    }

    [Fact]
    public void GetLowStockProducts_WithThreshold_ShouldReturnProductsBelowThreshold()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product1 = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var product2 = CoffeeProduct.Create("House Roast", "Local Roasters", CoffeeType.Filter);

        coffeeStock.AddPurchase(product1, Weight.FromGrams(1000), Money.Create(25.99m, "USD"), "Vendor", _userId, _clock.UtcNow.AddDays(-1), _clock);
        coffeeStock.AddPurchase(product2, Weight.FromGrams(500), Money.Create(15.50m, "USD"), "Vendor", _userId, _clock.UtcNow.AddDays(-1), _clock);

        coffeeStock.ConsumeStock(product1, Weight.FromGrams(950), _clock);
        coffeeStock.ConsumeStock(product2, Weight.FromGrams(100), _clock);

        var threshold = Weight.FromGrams(100);

        var lowStockProducts = coffeeStock.GetLowStockProducts(threshold).ToList();

        lowStockProducts.Should().HaveCount(1);
        lowStockProducts.First().Product.Should().Be(product1);
        lowStockProducts.First().CurrentStock.Should().Be(Weight.FromGrams(50));
    }

    [Fact]
    public void GetLowStockProducts_WithHighThreshold_ShouldReturnAllProducts()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);

        coffeeStock.AddPurchase(product, Weight.FromGrams(500), Money.Create(25.99m, "USD"), "Vendor", _userId, _clock.UtcNow.AddDays(-1), _clock);

        var highThreshold = Weight.FromGrams(1000);

        var lowStockProducts = coffeeStock.GetLowStockProducts(highThreshold).ToList();

        lowStockProducts.Should().HaveCount(1);
        lowStockProducts.First().CurrentStock.Should().Be(Weight.FromGrams(500));
    }
}