using BeanShare.Domain.Aggregates.BillingPeriod;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Seeds;

public class BillingPeriodSeeder : IDataSeeder
{
    public int Order => 5;

    public async Task SeedAsync(BeanShareDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.BillingPeriods.AnyAsync(cancellationToken))
            return;

        var clock = new SeedClock(DateTime.UtcNow);
        var billingPeriods = CreateBillingPeriods(clock);

        foreach (var period in billingPeriods)
        {
            await context.BillingPeriods.AddAsync(period, cancellationToken);
        }
        await context.SaveChangesAsync(cancellationToken);

        await AssignConsumptionsToBillingPeriods(context, cancellationToken);
    }

    private static List<BillingPeriod> CreateBillingPeriods(SeedClock clock)
    {
        var periods = new List<BillingPeriod>();
        var now = clock.UtcNow;

        var engineeringSpaceId = new SpaceId(new Guid("aaaa1111-aaaa-1111-aaaa-111111111111"));
        var johnSmithId = new UserId(new Guid("11111111-1111-1111-1111-111111111111"));

        var period1Start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-3);
        var period1End = period1Start.AddMonths(1).AddDays(-1);
        var period1 = BillingPeriod.Create(
            engineeringSpaceId,
            GetMonthName(period1Start),
            period1Start,
            period1End,
            johnSmithId,
            new SeedClock(period1Start)
        );
        period1.Open(johnSmithId, new SeedClock(period1Start.AddDays(1)));
        period1.Close(johnSmithId, new SeedClock(period1End.AddDays(1)));
        period1.MarkAsSettled(johnSmithId, new SeedClock(period1End.AddDays(3)));
        periods.Add(period1);

        var period2Start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-2);
        var period2End = period2Start.AddMonths(1).AddDays(-1);
        var period2 = BillingPeriod.Create(
            engineeringSpaceId,
            GetMonthName(period2Start),
            period2Start,
            period2End,
            johnSmithId,
            new SeedClock(period2Start)
        );
        period2.Open(johnSmithId, new SeedClock(period2Start.AddDays(1)));
        period2.Close(johnSmithId, new SeedClock(period2End.AddDays(1)));
        period2.MarkAsSettled(johnSmithId, new SeedClock(period2End.AddDays(3)));
        periods.Add(period2);

        var period3Start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
        var period3End = period3Start.AddMonths(1).AddDays(-1);
        var period3 = BillingPeriod.Create(
            engineeringSpaceId,
            GetMonthName(period3Start),
            period3Start,
            period3End,
            johnSmithId,
            new SeedClock(period3Start)
        );
        period3.Open(johnSmithId, new SeedClock(period3Start.AddDays(1)));
        period3.Close(johnSmithId, new SeedClock(period3End.AddDays(1)));
        periods.Add(period3);

        var period4Start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var period4End = period4Start.AddMonths(1).AddDays(-1);
        var period4 = BillingPeriod.Create(
            engineeringSpaceId,
            GetMonthName(period4Start),
            period4Start,
            period4End,
            johnSmithId,
            new SeedClock(period4Start)
        );
        period4.Open(johnSmithId, new SeedClock(period4Start.AddDays(1)));
        periods.Add(period4);
d
        var marketingSpaceId = new SpaceId(new Guid("bbbb2222-bbbb-2222-bbbb-222222222222"));
        var janeId = new UserId(new Guid("22222222-2222-2222-2222-222222222222"));

        var marketingPeriod = BillingPeriod.Create(
            marketingSpaceId,
            GetMonthName(period2Start),
            period2Start,
            period2End,
            janeId,
            new SeedClock(period2Start)
        );
        marketingPeriod.Open(janeId, new SeedClock(period2Start.AddDays(1)));
        periods.Add(marketingPeriod);

        return periods;
    }

    private static string GetMonthName(DateTime date)
    {
        return date.ToString("MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static async Task AssignConsumptionsToBillingPeriods(BeanShareDbContext context, CancellationToken cancellationToken)
    {
        var billingPeriods = await context.BillingPeriods
            .Where(bp => bp.State != Domain.Enums.BillingState.Draft)
            .ToListAsync(cancellationToken);

        var consumptions = await context.Consumptions
            .Where(c => c.BillingPeriodId == null)
            .ToListAsync(cancellationToken);

        foreach (var consumption in consumptions)
        {
            var matchingPeriod = billingPeriods
                .Where(bp => bp.SpaceId == consumption.SpaceId)
                .FirstOrDefault(bp => bp.ContainsDate(consumption.ConsumedAt));

            if (matchingPeriod != null)
            {
                consumption.AssignToBillingPeriod(matchingPeriod.Id);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private class SeedClock : IClock
    {
        public DateTime UtcNow { get; }

        public SeedClock(DateTime utcNow)
        {
            UtcNow = utcNow;
        }
    }
}
