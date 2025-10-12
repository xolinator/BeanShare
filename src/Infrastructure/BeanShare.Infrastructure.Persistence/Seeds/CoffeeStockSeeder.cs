using BeanShare.Domain.Aggregates;
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

        var coffeeStocks = GetSeedCoffeeStocks();
        await context.CoffeeStocks.AddRangeAsync(coffeeStocks, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<CoffeeStock> GetSeedCoffeeStocks()
    {
        var stocks = new List<CoffeeStock>();

        var engineeringStock = CoffeeStock.Create(
            new SpaceId(new Guid("aaaa1111-aaaa-1111-aaaa-111111111111"))
        );

        engineeringStock.AddPurchase(
            new UserId(new Guid("11111111-1111-1111-1111-111111111111")), // John
            new CoffeeInfo("Lavazza Super Crema", "Espresso", "Italy"),
            new Quantity(1000, "g"),
            new Money(25.99m, "USD"),
            DateTime.UtcNow.AddDays(-30)
        );

        engineeringStock.AddPurchase(
            new UserId(new Guid("44444444-4444-4444-4444-444444444444")), // Emma
            new CoffeeInfo("Starbucks Pike Place", "Medium Roast", "USA"),
            new Quantity(500, "g"),
            new Money(12.99m, "USD"),
            DateTime.UtcNow.AddDays(-20)
        );

        engineeringStock.AddPurchase(
            new UserId(new Guid("33333333-3333-3333-3333-333333333333")), // Mike
            new CoffeeInfo("Blue Mountain", "Premium Blend", "Jamaica"),
            new Quantity(250, "g"),
            new Money(45.00m, "USD"),
            DateTime.UtcNow.AddDays(-10)
        );

        engineeringStock.AddPurchase(
            new UserId(new Guid("55555555-5555-5555-5555-555555555555")), // Alex
            new CoffeeInfo("Ethiopian Yirgacheffe", "Single Origin", "Ethiopia"),
            new Quantity(750, "g"),
            new Money(32.50m, "USD"),
            DateTime.UtcNow.AddDays(-5)
        );

        stocks.Add(engineeringStock);

        var marketingStock = CoffeeStock.Create(
            new SpaceId(new Guid("bbbb2222-bbbb-2222-bbbb-222222222222"))
        );

        marketingStock.AddPurchase(
            new UserId(new Guid("22222222-2222-2222-2222-222222222222")), // Sarah
            new CoffeeInfo("Illy Classico", "Medium Roast", "Italy"),
            new Quantity(500, "g"),
            new Money(18.99m, "USD"),
            DateTime.UtcNow.AddDays(-25)
        );

        marketingStock.AddPurchase(
            new UserId(new Guid("66666666-6666-6666-6666-666666666666")), // Lisa
            new CoffeeInfo("Peet's Major Dickason", "Dark Roast", "USA"),
            new Quantity(750, "g"),
            new Money(22.99m, "USD"),
            DateTime.UtcNow.AddDays(-12)
        );

        marketingStock.AddPurchase(
            new UserId(new Guid("44444444-4444-4444-4444-444444444444")), // Emma
            new CoffeeInfo("Costa Rica Tarrazú", "Single Origin", "Costa Rica"),
            new Quantity(500, "g"),
            new Money(28.00m, "USD"),
            DateTime.UtcNow.AddDays(-3)
        );

        stocks.Add(marketingStock);

        var remoteStock = CoffeeStock.Create(
            new SpaceId(new Guid("cccc3333-cccc-3333-cccc-333333333333"))
        );

        remoteStock.AddPurchase(
            new UserId(new Guid("33333333-3333-3333-3333-333333333333")), // Mike
            new CoffeeInfo("Death Wish Coffee", "Extra Strong", "USA"),
            new Quantity(450, "g"),
            new Money(19.99m, "USD"),
            DateTime.UtcNow.AddDays(-28)
        );

        remoteStock.AddPurchase(
            new UserId(new Guid("88888888-8888-8888-8888-888888888888")), // Test User
            new CoffeeInfo("Green Mountain", "Breakfast Blend", "USA"),
            new Quantity(600, "g"),
            new Money(14.99m, "USD"),
            DateTime.UtcNow.AddDays(-15)
        );

        remoteStock.AddPurchase(
            new UserId(new Guid("55555555-5555-5555-5555-555555555555")), // Alex
            new CoffeeInfo("Colombian Supremo", "Medium-Dark", "Colombia"),
            new Quantity(1000, "g"),
            new Money(35.00m, "USD"),
            DateTime.UtcNow.AddDays(-7)
        );

        remoteStock.AddPurchase(
            new UserId(new Guid("66666666-6666-6666-6666-666666666666")), // Lisa
            new CoffeeInfo("Kona Coffee", "Premium Hawaiian", "USA"),
            new Quantity(250, "g"),
            new Money(55.00m, "USD"),
            DateTime.UtcNow.AddDays(-2)
        );

        stocks.Add(remoteStock);


        var startupStock = CoffeeStock.Create(
            new SpaceId(new Guid("dddd4444-dddd-4444-dddd-444444444444"))
        );

        startupStock.AddPurchase(
            new UserId(new Guid("55555555-5555-5555-5555-555555555555")), // Alex
            new CoffeeInfo("Folgers Classic", "Medium Roast", "USA"),
            new Quantity(920, "g"),
            new Money(8.99m, "USD"),
            DateTime.UtcNow.AddDays(-21)
        );

        startupStock.AddPurchase(
            new UserId(new Guid("33333333-3333-3333-3333-333333333333")), // Mike
            new CoffeeInfo("Store Brand Dark", "Dark Roast", "Generic"),
            new Quantity(1000, "g"),
            new Money(6.99m, "USD"),
            DateTime.UtcNow.AddDays(-8)
        );

        stocks.Add(startupStock);

        var executiveStock = CoffeeStock.Create(
            new SpaceId(new Guid("eeee5555-eeee-5555-eeee-555555555555"))
        );

        executiveStock.AddPurchase(
            new UserId(new Guid("11111111-1111-1111-1111-111111111111")), // John
            new CoffeeInfo("Jamaica Blue Mountain", "Premium Reserve", "Jamaica"),
            new Quantity(500, "g"),
            new Money(125.00m, "USD"),
            DateTime.UtcNow.AddDays(-14)
        );

        executiveStock.AddPurchase(
            new UserId(new Guid("22222222-2222-2222-2222-222222222222")), // Sarah
            new CoffeeInfo("Hawaiian Kona Extra Fancy", "100% Kona", "USA"),
            new Quantity(450, "g"),
            new Money(95.00m, "USD"),
            DateTime.UtcNow.AddDays(-6)
        );

        executiveStock.AddPurchase(
            new UserId(new Guid("44444444-4444-4444-4444-444444444444")), // Emma
            new CoffeeInfo("Geisha Panama", "Ultra Premium", "Panama"),
            new Quantity(250, "g"),
            new Money(150.00m, "USD"),
            DateTime.UtcNow.AddDays(-1)
        );

        stocks.Add(executiveStock);

        return stocks;
    }
}