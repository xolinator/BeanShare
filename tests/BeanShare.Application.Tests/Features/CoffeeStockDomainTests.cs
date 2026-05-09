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
        averageCost.Currency.Code.Should().Be("USD");
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
        averageCost.Currency.Code.Should().Be("USD");
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

    [Fact]
    public void UpdatePurchase_IncreasingQuantity_ShouldGrowStockLevel()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var purchasedAt = _clock.UtcNow.AddDays(-1);
        coffeeStock.AddPurchase(product, Weight.FromGrams(1000), Money.Create(20m, "USD"), "Vendor", _userId, purchasedAt, _clock);
        var purchaseId = coffeeStock.Purchases.First().Id;

        coffeeStock.UpdatePurchase(purchaseId, Weight.FromGrams(1500), 30m, purchasedAt, _clock);

        var stock = coffeeStock.StockLevels.First();
        stock.TotalPurchased.Should().Be(Weight.FromGrams(1500));
        stock.CurrentStock.Should().Be(Weight.FromGrams(1500));
        coffeeStock.Purchases.First().Quantity.Should().Be(Weight.FromGrams(1500));
    }

    [Fact]
    public void UpdatePurchase_DecreasingQuantity_ShouldShrinkStockLevel()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var purchasedAt = _clock.UtcNow.AddDays(-1);
        coffeeStock.AddPurchase(product, Weight.FromGrams(1000), Money.Create(20m, "USD"), "Vendor", _userId, purchasedAt, _clock);
        var purchaseId = coffeeStock.Purchases.First().Id;

        coffeeStock.UpdatePurchase(purchaseId, Weight.FromGrams(400), 8m, purchasedAt, _clock);

        var stock = coffeeStock.StockLevels.First();
        stock.TotalPurchased.Should().Be(Weight.FromGrams(400));
        stock.CurrentStock.Should().Be(Weight.FromGrams(400));
    }

    [Fact]
    public void UpdatePurchase_DecreasingBelowConsumed_ShouldCapAtAvailable()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var purchasedAt = _clock.UtcNow.AddDays(-1);
        coffeeStock.AddPurchase(product, Weight.FromGrams(1000), Money.Create(20m, "USD"), "Vendor", _userId, purchasedAt, _clock);
        coffeeStock.ConsumeStock(product, Weight.FromGrams(700), _clock);
        var purchaseId = coffeeStock.Purchases.First().Id;

        coffeeStock.UpdatePurchase(purchaseId, Weight.FromGrams(100), 2m, purchasedAt, _clock);

        var stock = coffeeStock.StockLevels.First();
        stock.TotalPurchased.Should().Be(Weight.FromGrams(700));
        stock.TotalConsumed.Should().Be(Weight.FromGrams(700));
        stock.CurrentStock.Should().Be(Weight.Zero);
    }

    [Fact]
    public void UpdatePurchase_ChangingProductAndQuantity_ShouldReduceOldStockByOldQuantity()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var oldProduct = CoffeeProduct.Create("Pike Place", "Starbucks", CoffeeType.Filter);
        var newProduct = CoffeeProduct.Create("Super Crema", "Lavazza", CoffeeType.Espresso);
        var purchasedAt = _clock.UtcNow.AddDays(-1);
        coffeeStock.AddPurchase(oldProduct, Weight.FromGrams(1000), Money.Create(20m, "USD"), "Vendor", _userId, purchasedAt, _clock);
        var purchaseId = coffeeStock.Purchases.First().Id;

        coffeeStock.UpdatePurchase(
            purchaseId,
            Weight.FromGrams(500),
            15m,
            purchasedAt,
            _clock,
            newProductName: newProduct.Name,
            newProductBrand: newProduct.Brand,
            newCoffeeType: newProduct.Type);

        var stockOld = coffeeStock.StockLevels.Single(sl => sl.Product.Equals(oldProduct));
        var stockNew = coffeeStock.StockLevels.Single(sl => sl.Product.Equals(newProduct));
        stockOld.CurrentStock.Should().Be(Weight.Zero);
        stockOld.TotalPurchased.Should().Be(Weight.Zero);
        stockNew.TotalPurchased.Should().Be(Weight.FromGrams(500));
        stockNew.CurrentStock.Should().Be(Weight.FromGrams(500));
        coffeeStock.Purchases.Single().Product.Should().Be(newProduct);
    }

    [Fact]
    public void UpdatePurchase_ChangingProductOnly_ShouldMoveStockToNewProduct()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var oldProduct = CoffeeProduct.Create("Pike Place", "Starbucks", CoffeeType.Filter);
        var newProduct = CoffeeProduct.Create("Super Crema", "Lavazza", CoffeeType.Espresso);
        var purchasedAt = _clock.UtcNow.AddDays(-1);
        coffeeStock.AddPurchase(oldProduct, Weight.FromGrams(800), Money.Create(16m, "USD"), "Vendor", _userId, purchasedAt, _clock);
        var purchaseId = coffeeStock.Purchases.First().Id;

        coffeeStock.UpdatePurchase(
            purchaseId,
            Weight.FromGrams(800),
            16m,
            purchasedAt,
            _clock,
            newProductName: newProduct.Name,
            newProductBrand: newProduct.Brand,
            newCoffeeType: newProduct.Type);

        var stockOld = coffeeStock.StockLevels.Single(sl => sl.Product.Equals(oldProduct));
        var stockNew = coffeeStock.StockLevels.Single(sl => sl.Product.Equals(newProduct));
        stockOld.CurrentStock.Should().Be(Weight.Zero);
        stockNew.CurrentStock.Should().Be(Weight.FromGrams(800));
    }

    [Fact]
    public void UpdatePurchase_NonexistentPurchase_ShouldThrow()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var purchasedAt = _clock.UtcNow.AddDays(-1);

        var act = () => coffeeStock.UpdatePurchase(Guid.NewGuid(), Weight.FromGrams(100), 5m, purchasedAt, _clock);

        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public void DeletePurchase_WithValidPurchase_ShouldRemoveAndReduceStockLevel()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var purchasedAt = _clock.UtcNow.AddDays(-1);
        coffeeStock.AddPurchase(product, Weight.FromGrams(600), Money.Create(12m, "USD"), "Vendor", _userId, purchasedAt, _clock);
        var purchaseId = coffeeStock.Purchases.First().Id;

        coffeeStock.DeletePurchase(purchaseId, _clock);

        coffeeStock.Purchases.Should().BeEmpty();
        var stock = coffeeStock.StockLevels.First();
        stock.TotalPurchased.Should().Be(Weight.Zero);
        stock.CurrentStock.Should().Be(Weight.Zero);
    }

    [Fact]
    public void DeletePurchase_WithPartiallyConsumedStock_ShouldCapReductionAtAvailable()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Premium Blend", "Blue Mountain", CoffeeType.Espresso);
        var purchasedAt = _clock.UtcNow.AddDays(-1);
        coffeeStock.AddPurchase(product, Weight.FromGrams(1000), Money.Create(20m, "USD"), "Vendor", _userId, purchasedAt, _clock);
        coffeeStock.ConsumeStock(product, Weight.FromGrams(400), _clock);
        var purchaseId = coffeeStock.Purchases.First().Id;

        coffeeStock.DeletePurchase(purchaseId, _clock);

        coffeeStock.Purchases.Should().BeEmpty();
        var stock = coffeeStock.StockLevels.First();
        stock.TotalPurchased.Should().Be(Weight.FromGrams(400));
        stock.TotalConsumed.Should().Be(Weight.FromGrams(400));
        stock.CurrentStock.Should().Be(Weight.Zero);
    }

    [Fact]
    public void DeletePurchase_NonexistentPurchase_ShouldThrow()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);

        var act = () => coffeeStock.DeletePurchase(Guid.NewGuid(), _clock);

        act.Should().Throw<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public void SetCurrentlyUsed_WithValidStockLevel_ShouldMarkAsCurrentlyUsed()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Ethiopian Yirgacheffe", "Onyx", CoffeeType.Filter);
        coffeeStock.AddPurchase(product, Weight.FromGrams(500), Money.Create(18.50m, "USD"), "Roaster Direct", _userId, _clock.UtcNow.AddDays(-1), _clock);
        var stockLevelId = coffeeStock.StockLevels.First().Id;

        coffeeStock.SetCurrentlyUsed(stockLevelId, _clock);

        coffeeStock.StockLevels.First().IsCurrentlyUsed.Should().BeTrue();
    }

    [Fact]
    public void SetCurrentlyUsed_ShouldClearPreviouslyMarkedStockLevel()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product1 = CoffeeProduct.Create("Colombian Supremo", "Counter Culture", CoffeeType.Espresso);
        var product2 = CoffeeProduct.Create("Kenyan AA", "Heart Roasters", CoffeeType.Filter);
        coffeeStock.AddPurchase(product1, Weight.FromGrams(1000), Money.Create(22m, "USD"), "Importer", _userId, _clock.UtcNow.AddDays(-2), _clock);
        coffeeStock.AddPurchase(product2, Weight.FromGrams(750), Money.Create(19m, "USD"), "Importer", _userId, _clock.UtcNow.AddDays(-1), _clock);
        var firstId = coffeeStock.StockLevels.First().Id;
        var secondId = coffeeStock.StockLevels.Last().Id;

        coffeeStock.SetCurrentlyUsed(firstId, _clock);
        coffeeStock.SetCurrentlyUsed(secondId, _clock);

        coffeeStock.StockLevels.Single(sl => sl.Id == firstId).IsCurrentlyUsed.Should().BeFalse();
        coffeeStock.StockLevels.Single(sl => sl.Id == secondId).IsCurrentlyUsed.Should().BeTrue();
    }

    [Fact]
    public void SetCurrentlyUsed_OnArchivedStockLevel_ShouldThrowInvalidOperation()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Guatemalan Antigua", "Verve", CoffeeType.Espresso);
        coffeeStock.AddPurchase(product, Weight.FromGrams(500), Money.Create(16m, "USD"), "Distributor", _userId, _clock.UtcNow.AddDays(-1), _clock);
        var stockLevelId = coffeeStock.StockLevels.First().Id;
        coffeeStock.ArchiveStockLevel(stockLevelId, _clock);

        var act = () => coffeeStock.SetCurrentlyUsed(stockLevelId, _clock);

        act.Should().Throw<InvalidOperationException>().WithMessage("*archived*");
    }

    [Fact]
    public void SetCurrentlyUsed_WithNonexistentStockLevel_ShouldThrowArgument()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);

        var act = () => coffeeStock.SetCurrentlyUsed(Guid.NewGuid(), _clock);

        act.Should().Throw<ArgumentException>().WithMessage("*not found*");
    }

    [Fact]
    public void ClearCurrentlyUsed_ShouldClearAllMarkedStockLevels()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Sumatra Mandheling", "Stumptown", CoffeeType.Espresso);
        coffeeStock.AddPurchase(product, Weight.FromGrams(800), Money.Create(21m, "USD"), "Supplier", _userId, _clock.UtcNow.AddDays(-1), _clock);
        var stockLevelId = coffeeStock.StockLevels.First().Id;
        coffeeStock.SetCurrentlyUsed(stockLevelId, _clock);

        coffeeStock.ClearCurrentlyUsed(_clock);

        coffeeStock.StockLevels.All(sl => !sl.IsCurrentlyUsed).Should().BeTrue();
    }

    [Fact]
    public void ArchiveStockLevel_WhenCurrentlyUsed_ShouldClearCurrentlyUsedFlag()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Costa Rican Tarrazu", "Intelligentsia", CoffeeType.Filter);
        coffeeStock.AddPurchase(product, Weight.FromGrams(600), Money.Create(17m, "USD"), "Direct Trade", _userId, _clock.UtcNow.AddDays(-1), _clock);
        var stockLevelId = coffeeStock.StockLevels.First().Id;
        coffeeStock.SetCurrentlyUsed(stockLevelId, _clock);

        coffeeStock.ArchiveStockLevel(stockLevelId, _clock);

        var archived = coffeeStock.StockLevels.First();
        archived.IsArchived.Should().BeTrue();
        archived.IsCurrentlyUsed.Should().BeFalse();
    }

    [Fact]
    public void StockLevel_SetCurrentlyUsed_WhenArchived_ShouldThrowDirectly()
    {
        var coffeeStock = CoffeeStock.Create(_spaceId, _clock);
        var product = CoffeeProduct.Create("Rwandan Bourbon", "George Howell", CoffeeType.Filter);
        coffeeStock.AddPurchase(product, Weight.FromGrams(400), Money.Create(14m, "USD"), "Coop", _userId, _clock.UtcNow.AddDays(-1), _clock);
        var stockLevel = coffeeStock.StockLevels.First();
        stockLevel.Archive(_clock);

        var act = () => stockLevel.SetCurrentlyUsed(_clock);

        act.Should().Throw<InvalidOperationException>().WithMessage("*archived*");
    }
}