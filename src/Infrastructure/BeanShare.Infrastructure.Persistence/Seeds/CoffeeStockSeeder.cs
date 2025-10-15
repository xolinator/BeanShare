using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Seeds;

public class CoffeeStockSeeder : IDataSeeder
{
    public int Order => 3;

    public async Task SeedAsync(BeanShareDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.CoffeeStocks.AnyAsync(cancellationToken))
            return;

        var clock = new FixedClock(DateTime.UtcNow);
        var coffeeStocks = GetSeedCoffeeStocks(clock);
        await context.CoffeeStocks.AddRangeAsync(coffeeStocks, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<CoffeeStock> GetSeedCoffeeStocks(IClock clock)
    {
        var stocks = new List<CoffeeStock>();

        var engineeringStock = CoffeeStock.Create(
            new SpaceId(new Guid("aaaa1111-aaaa-1111-aaaa-111111111111")),
            clock
        );

        engineeringStock.AddPurchase(
            CoffeeProduct.Create("Super Crema", "Lavazza", CoffeeType.Espresso),
            Weight.FromGrams(1000),
            Money.Create(25.99m, "USD"),
            "Coffee Supplier Inc",
            new UserId(new Guid("11111111-1111-1111-1111-111111111111")), // John
            DateTime.UtcNow.AddDays(-30),
            clock
        );

        engineeringStock.AddPurchase(
            CoffeeProduct.Create("Pike Place", "Starbucks", CoffeeType.Filter),
            Weight.FromGrams(500),
            Money.Create(12.99m, "USD"),
            "Coffee Supplier Inc",
            new UserId(new Guid("44444444-4444-4444-4444-444444444444")), // Emma
            DateTime.UtcNow.AddDays(-20),
            clock
        );

        engineeringStock.AddPurchase(
            CoffeeProduct.Create("Blue Mountain Premium", "Jamaica", CoffeeType.Specialty),
            Weight.FromGrams(250),
            Money.Create(45.00m, "USD"),
            "Premium Coffee Co",
            new UserId(new Guid("33333333-3333-3333-3333-333333333333")), // Mike
            DateTime.UtcNow.AddDays(-10),
            clock
        );

        engineeringStock.AddPurchase(
            CoffeeProduct.Create("Yirgacheffe", "Ethiopian Origin", CoffeeType.Specialty),
            Weight.FromGrams(750),
            Money.Create(32.50m, "USD"),
            "Specialty Roasters",
            new UserId(new Guid("55555555-5555-5555-5555-555555555555")), // Alex
            DateTime.UtcNow.AddDays(-5),
            clock
        );

        stocks.Add(engineeringStock);

        var marketingStock = CoffeeStock.Create(
            new SpaceId(new Guid("bbbb2222-bbbb-2222-bbbb-222222222222")),
            clock
        );

        marketingStock.AddPurchase(
            CoffeeProduct.Create("Classico", "Illy", CoffeeType.Filter),
            Weight.FromGrams(500),
            Money.Create(18.99m, "USD"),
            "Coffee Supplier Inc",
            new UserId(new Guid("22222222-2222-2222-2222-222222222222")), // Sarah
            DateTime.UtcNow.AddDays(-25),
            clock
        );

        marketingStock.AddPurchase(
            CoffeeProduct.Create("Major Dickason's Blend", "Peet's", CoffeeType.Filter),
            Weight.FromGrams(750),
            Money.Create(22.99m, "USD"),
            "Coffee Supplier Inc",
            new UserId(new Guid("66666666-6666-6666-6666-666666666666")), // Lisa
            DateTime.UtcNow.AddDays(-12),
            clock
        );

        marketingStock.AddPurchase(
            CoffeeProduct.Create("Tarrazú", "Costa Rica", CoffeeType.Specialty),
            Weight.FromGrams(500),
            Money.Create(28.00m, "USD"),
            "Specialty Roasters",
            new UserId(new Guid("44444444-4444-4444-4444-444444444444")), // Emma
            DateTime.UtcNow.AddDays(-3),
            clock
        );

        stocks.Add(marketingStock);

        var remoteStock = CoffeeStock.Create(
            new SpaceId(new Guid("cccc3333-cccc-3333-cccc-333333333333")),
            clock
        );

        remoteStock.AddPurchase(
            CoffeeProduct.Create("Death Wish", "Death Wish Coffee", CoffeeType.Espresso),
            Weight.FromGrams(450),
            Money.Create(19.99m, "USD"),
            "Online Coffee Store",
            new UserId(new Guid("33333333-3333-3333-3333-333333333333")), // Mike
            DateTime.UtcNow.AddDays(-28),
            clock
        );

        remoteStock.AddPurchase(
            CoffeeProduct.Create("Breakfast Blend", "Green Mountain", CoffeeType.Filter),
            Weight.FromGrams(600),
            Money.Create(14.99m, "USD"),
            "Online Coffee Store",
            new UserId(new Guid("88888888-8888-8888-8888-888888888888")), // Test User
            DateTime.UtcNow.AddDays(-15),
            clock
        );

        remoteStock.AddPurchase(
            CoffeeProduct.Create("Supremo", "Colombian", CoffeeType.Filter),
            Weight.FromGrams(1000),
            Money.Create(35.00m, "USD"),
            "Coffee Supplier Inc",
            new UserId(new Guid("55555555-5555-5555-5555-555555555555")), // Alex
            DateTime.UtcNow.AddDays(-7),
            clock
        );

        remoteStock.AddPurchase(
            CoffeeProduct.Create("Kona Premium", "Hawaiian", CoffeeType.Specialty),
            Weight.FromGrams(250),
            Money.Create(55.00m, "USD"),
            "Premium Coffee Co",
            new UserId(new Guid("66666666-6666-6666-6666-666666666666")), // Lisa
            DateTime.UtcNow.AddDays(-2),
            clock
        );

        stocks.Add(remoteStock);

        var startupStock = CoffeeStock.Create(
            new SpaceId(new Guid("dddd4444-dddd-4444-dddd-444444444444")),
            clock
        );

        startupStock.AddPurchase(
            CoffeeProduct.Create("Classic Roast", "Folgers", CoffeeType.Filter),
            Weight.FromGrams(920),
            Money.Create(8.99m, "USD"),
            "Grocery Store",
            new UserId(new Guid("55555555-5555-5555-5555-555555555555")), // Alex
            DateTime.UtcNow.AddDays(-21),
            clock
        );

        startupStock.AddPurchase(
            CoffeeProduct.Create("Dark Roast", "Store Brand", CoffeeType.Filter),
            Weight.FromGrams(1000),
            Money.Create(6.99m, "USD"),
            "Grocery Store",
            new UserId(new Guid("33333333-3333-3333-3333-333333333333")), // Mike
            DateTime.UtcNow.AddDays(-8),
            clock
        );

        stocks.Add(startupStock);

        var executiveStock = CoffeeStock.Create(
            new SpaceId(new Guid("eeee5555-eeee-5555-eeee-555555555555")),
            clock
        );

        executiveStock.AddPurchase(
            CoffeeProduct.Create("Blue Mountain Reserve", "Jamaica", CoffeeType.Specialty),
            Weight.FromGrams(500),
            Money.Create(125.00m, "USD"),
            "Premium Coffee Co",
            new UserId(new Guid("11111111-1111-1111-1111-111111111111")), // John
            DateTime.UtcNow.AddDays(-14),
            clock
        );

        executiveStock.AddPurchase(
            CoffeeProduct.Create("Extra Fancy Kona", "Hawaiian", CoffeeType.Specialty),
            Weight.FromGrams(450),
            Money.Create(95.00m, "USD"),
            "Premium Coffee Co",
            new UserId(new Guid("22222222-2222-2222-2222-222222222222")), // Sarah
            DateTime.UtcNow.AddDays(-6),
            clock
        );

        executiveStock.AddPurchase(
            CoffeeProduct.Create("Geisha", "Panama", CoffeeType.Specialty),
            Weight.FromGrams(250),
            Money.Create(150.00m, "USD"),
            "Ultra Premium Roasters",
            new UserId(new Guid("44444444-4444-4444-4444-444444444444")), // Emma
            DateTime.UtcNow.AddDays(-1),
            clock
        );

        stocks.Add(executiveStock);

        return stocks;
    }

    private class FixedClock : IClock
    {
        public DateTime UtcNow { get; }

        public FixedClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }
    }
}
