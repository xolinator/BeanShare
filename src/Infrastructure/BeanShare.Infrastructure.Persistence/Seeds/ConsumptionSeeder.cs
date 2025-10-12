using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Seeds;

public class ConsumptionSeeder : IDataSeeder
{
    public int Order => 4;

    public async Task SeedAsync(BeanShareDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.ConsumptionEntries.AnyAsync(cancellationToken))
            return;

        var consumptionEntries = GetSeedConsumptionEntries();
        await context.ConsumptionEntries.AddRangeAsync(consumptionEntries, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<ConsumptionEntry> GetSeedConsumptionEntries()
    {
        var entries = new List<ConsumptionEntry>();
        var random = new Random(42);

        var engineeringSpaceId = new SpaceId(new Guid("aaaa1111-aaaa-1111-aaaa-111111111111"));

        for (int daysAgo = 30; daysAgo >= 0; daysAgo--)
        {
            var date = DateTime.UtcNow.AddDays(-daysAgo).Date;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                if (random.Next(100) > 30) continue;
            }

            AddUserConsumption(entries, engineeringSpaceId,
                new UserId(new Guid("11111111-1111-1111-1111-111111111111")),
                date, 3 + random.Next(2), "Strong preference for espresso");

            if (random.Next(100) > 10)
                AddUserConsumption(entries, engineeringSpaceId,
                    new UserId(new Guid("22222222-2222-2222-2222-222222222222")),
                    date, 2 + random.Next(2), "Cappuccino lover");

            if (random.Next(100) > 20)
                AddUserConsumption(entries, engineeringSpaceId,
                    new UserId(new Guid("33333333-3333-3333-3333-333333333333")),
                    date, 2, "Black coffee only");

            if (random.Next(100) > 15)
                AddUserConsumption(entries, engineeringSpaceId,
                    new UserId(new Guid("44444444-4444-4444-4444-444444444444")),
                    date, 1 + random.Next(2), "Prefers latte");

            if (random.Next(100) > 25)
                AddUserConsumption(entries, engineeringSpaceId,
                    new UserId(new Guid("55555555-5555-5555-5555-555555555555")),
                    date, random.Next(4), "Experimental with coffee types");
        }

        var marketingSpaceId = new SpaceId(new Guid("bbbb2222-bbbb-2222-bbbb-222222222222"));

        for (int daysAgo = 25; daysAgo >= 0; daysAgo--)
        {
            var date = DateTime.UtcNow.AddDays(-daysAgo).Date;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            AddUserConsumption(entries, marketingSpaceId,
                new UserId(new Guid("22222222-2222-2222-2222-222222222222")),
                date, 2 + random.Next(2), "Morning meetings need coffee");

            if (random.Next(100) > 50)
                AddUserConsumption(entries, marketingSpaceId,
                    new UserId(new Guid("44444444-4444-4444-4444-444444444444")),
                    date, 1 + random.Next(2), "Afternoon coffee");

            if (random.Next(100) > 10)
                AddUserConsumption(entries, marketingSpaceId,
                    new UserId(new Guid("66666666-6666-6666-6666-666666666666")),
                    date, 2, "Consistent coffee routine");
        }

        var remoteSpaceId = new SpaceId(new Guid("cccc3333-cccc-3333-cccc-333333333333"));

        for (int daysAgo = 20; daysAgo >= 0; daysAgo--)
        {
            var date = DateTime.UtcNow.AddDays(-daysAgo).Date;

            if (random.Next(100) > 30)
            {
                AddUserConsumption(entries, remoteSpaceId,
                    new UserId(new Guid("33333333-3333-3333-3333-333333333333")),
                    date, 1 + random.Next(3), "Home office setup");
            }

            if (random.Next(100) > 40)
            {
                AddUserConsumption(entries, remoteSpaceId,
                    new UserId(new Guid("88888888-8888-8888-8888-888888888888")),
                    date, random.Next(4), "Testing different blends");
            }

            if (random.Next(100) > 60)
            {
                AddUserConsumption(entries, remoteSpaceId,
                    new UserId(new Guid("55555555-5555-5555-5555-555555555555")),
                    date, 1 + random.Next(2), "Afternoon boost");
            }
        }

        var startupSpaceId = new SpaceId(new Guid("dddd4444-dddd-4444-dddd-444444444444"));

        for (int daysAgo = 15; daysAgo >= 0; daysAgo--)
        {
            var date = DateTime.UtcNow.AddDays(-daysAgo).Date;
            if (random.Next(100) > 20) 
            {
                AddUserConsumption(entries, startupSpaceId,
                    new UserId(new Guid("55555555-5555-5555-5555-555555555555")),
                    date, 3 + random.Next(3), "Coding fuel");

                if (random.Next(100) > 30)
                    AddUserConsumption(entries, startupSpaceId,
                        new UserId(new Guid("33333333-3333-3333-3333-333333333333")),
                        date, 2 + random.Next(2), "Late night debugging");
            }
        }

        var executiveSpaceId = new SpaceId(new Guid("eeee5555-eeee-5555-eeee-555555555555"));

        for (int daysAgo = 10; daysAgo >= 0; daysAgo--)
        {
            var date = DateTime.UtcNow.AddDays(-daysAgo).Date;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            if (random.Next(100) > 20)
            {
                AddUserConsumption(entries, executiveSpaceId,
                    new UserId(new Guid("11111111-1111-1111-1111-111111111111")),
                    date, 1, "Morning executive briefing");
            }

            if (random.Next(100) > 30)
            {
                AddUserConsumption(entries, executiveSpaceId,
                    new UserId(new Guid("22222222-2222-2222-2222-222222222222")),
                    date, 1, "Board meeting preparation");
            }

            if (random.Next(100) > 40)
            {
                AddUserConsumption(entries, executiveSpaceId,
                    new UserId(new Guid("44444444-4444-4444-4444-444444444444")),
                    date, 1, "Strategic planning session");
            }
        }

        return entries;
    }

    private static void AddUserConsumption(
        List<ConsumptionEntry> entries,
        SpaceId spaceId,
        UserId userId,
        DateTime date,
        int cupsCount,
        string notes)
    {
        if (cupsCount <= 0) return;

        var coffeeTypes = new[] { "Espresso", "Cappuccino", "Latte", "Americano", "Flat White", "Macchiato" };
        var random = new Random((int)(date.Ticks % int.MaxValue) + userId.Value.GetHashCode());

        for (int i = 0; i < cupsCount; i++)
        {
            var hour = i switch
            {
                0 => 8 + random.Next(2),
                1 => 10 + random.Next(3),
                2 => 13 + random.Next(3),
                _ => 15 + random.Next(4)
            };

            var consumedAt = date.AddHours(hour).AddMinutes(random.Next(60));

            entries.Add(new ConsumptionEntry
            {
                Id = Guid.NewGuid(),
                SpaceId = spaceId,
                UserId = userId,
                ConsumedAt = consumedAt,
                Quantity = new Quantity(1, "cup"),
                CoffeeType = coffeeTypes[random.Next(coffeeTypes.Length)],
                Notes = i == 0 ? notes : null,
                CreatedAt = consumedAt
            });
        }
    }
}