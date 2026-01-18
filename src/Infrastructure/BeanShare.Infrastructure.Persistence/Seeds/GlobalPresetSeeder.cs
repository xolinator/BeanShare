using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Seeds;

public class GlobalPresetSeeder : IDataSeeder
{
    public int Order => 0;

    public async Task SeedAsync(BeanShareDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Set<GlobalPreset>().AnyAsync(cancellationToken))
            return;

        var presets = GetSeedGlobalPresets();
        await context.Set<GlobalPreset>().AddRangeAsync(presets, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static List<GlobalPreset> GetSeedGlobalPresets()
    {
        var now = DateTime.UtcNow;

        return new List<GlobalPreset>
        {
            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000001"),
                "Espresso",
                "Espresso Blend",
                "Espresso Machine",
                Weight.FromGrams(8),
                displayOrder: 1,
                createdAt: now,
                description: "A classic single shot of espresso (8g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000002"),
                "Double Espresso",
                "Espresso Blend",
                "Espresso Machine",
                Weight.FromGrams(16),
                displayOrder: 2,
                createdAt: now,
                description: "A double shot of espresso (16g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000003"),
                "Americano",
                "Espresso Blend",
                "Espresso Machine",
                Weight.FromGrams(8),
                displayOrder: 3,
                createdAt: now,
                description: "Espresso with hot water (8g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000004"),
                "Cappuccino",
                "Espresso Blend",
                "Espresso Machine",
                Weight.FromGrams(8),
                displayOrder: 4,
                createdAt: now,
                description: "Espresso with steamed milk and foam (8g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000005"),
                "Latte",
                "Espresso Blend",
                "Espresso Machine",
                Weight.FromGrams(8),
                displayOrder: 5,
                createdAt: now,
                description: "Espresso with lots of steamed milk (8g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000006"),
                "Filter Coffee",
                "Filter Roast",
                "Drip/Filter",
                Weight.FromGrams(15),
                displayOrder: 6,
                createdAt: now,
                description: "Standard drip/filter brew (15g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000007"),
                "Large Brew",
                "Filter Roast",
                "Drip/Filter",
                Weight.FromGrams(20),
                displayOrder: 7,
                createdAt: now,
                description: "Large filter coffee or mug (20g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000008"),
                "Pour Over",
                "Single Origin",
                "Pour Over",
                Weight.FromGrams(18),
                displayOrder: 8,
                createdAt: now,
                description: "Manual pour over brew (18g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000009"),
                "French Press",
                "Medium Roast",
                "French Press",
                Weight.FromGrams(25),
                displayOrder: 9,
                createdAt: now,
                description: "Single serving French press (25g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000010"),
                "Cold Brew",
                "Cold Brew Blend",
                "Cold Brew",
                Weight.FromGrams(30),
                displayOrder: 10,
                createdAt: now,
                description: "Cold brew concentrate serving (30g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000011"),
                "Moka Pot",
                "Espresso Blend",
                "Moka Pot",
                Weight.FromGrams(12),
                displayOrder: 11,
                createdAt: now,
                description: "Stovetop moka pot brew (12g)"
            ),

            GlobalPreset.CreateWithId(
                new Guid("aaaa0001-0001-0001-0001-000000000012"),
                "AeroPress",
                "Light Roast",
                "AeroPress",
                Weight.FromGrams(15),
                displayOrder: 12,
                createdAt: now,
                description: "AeroPress single serving (15g)"
            )
        };
    }
}
