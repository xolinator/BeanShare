using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.BillingPeriod;
using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.ValueObjects;
using System.Collections.Concurrent;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockBillingPeriodRepository : IBillingPeriodRepository
{
    private readonly ConcurrentDictionary<BillingPeriodId, BillingPeriod> _billingPeriods = new();
    private readonly IClock _clock;

    public MockBillingPeriodRepository(IClock clock)
    {
        _clock = clock;
        SeedData();
    }

    private void SeedData()
    {
        var spaceId = new SpaceId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var userId = new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        var period1 = BillingPeriod.Create(
            spaceId,
            "January 2025",
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 1, 31, 23, 59, 59, DateTimeKind.Utc),
            userId,
            _clock);

        _billingPeriods.TryAdd(period1.Id, period1);
    }

    public Task<BillingPeriod?> GetByIdAsync(BillingPeriodId id, CancellationToken cancellationToken = default)
    {
        _billingPeriods.TryGetValue(id, out var billingPeriod);
        return Task.FromResult(billingPeriod);
    }

    public Task<BillingPeriod?> GetActiveForSpaceAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        var activePeriod = _billingPeriods.Values
            .FirstOrDefault(bp => bp.SpaceId == spaceId && bp.State == BillingState.Open);
        return Task.FromResult(activePeriod);
    }

    public Task<IReadOnlyList<BillingPeriod>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        var periods = _billingPeriods.Values
            .Where(bp => bp.SpaceId == spaceId)
            .OrderByDescending(bp => bp.StartDate)
            .ToList();
        return Task.FromResult<IReadOnlyList<BillingPeriod>>(periods);
    }

    public Task AddAsync(BillingPeriod billingPeriod, CancellationToken cancellationToken = default)
    {
        _billingPeriods.TryAdd(billingPeriod.Id, billingPeriod);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(BillingPeriod billingPeriod, CancellationToken cancellationToken = default)
    {
        _billingPeriods.TryUpdate(billingPeriod.Id, billingPeriod, billingPeriod);
        return Task.CompletedTask;
    }

    public Task<bool> HasOverlappingPeriodAsync(
        SpaceId spaceId,
        DateTime startDate,
        DateTime endDate,
        BillingPeriodId? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var hasOverlap = _billingPeriods.Values
            .Where(bp => bp.SpaceId == spaceId)
            .Where(bp => excludeId == null || bp.Id != excludeId)
            .Any(bp =>
                (bp.StartDate <= startDate && bp.EndDate >= startDate) ||
                (bp.StartDate <= endDate && bp.EndDate >= endDate) ||
                (bp.StartDate >= startDate && bp.EndDate <= endDate));

        return Task.FromResult(hasOverlap);
    }
}