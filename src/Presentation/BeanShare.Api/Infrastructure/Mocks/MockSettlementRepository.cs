using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.Settlement;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using System.Collections.Concurrent;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockSettlementRepository : ISettlementRepository
{
    private readonly ConcurrentDictionary<SettlementId, Settlement> _settlements = new();
    private readonly IClock _clock;

    public MockSettlementRepository(IClock clock)
    {
        _clock = clock;
        SeedData();
    }

    private void SeedData()
    {
        // Start with empty settlements for now
    }

    public Task<Settlement?> GetByIdAsync(SettlementId id, CancellationToken cancellationToken = default)
    {
        _settlements.TryGetValue(id, out var settlement);
        return Task.FromResult(settlement);
    }

    public Task<Settlement?> GetByBillingPeriodIdAsync(BillingPeriodId billingPeriodId, CancellationToken cancellationToken = default)
    {
        var settlement = _settlements.Values.FirstOrDefault(s => s.BillingPeriodId == billingPeriodId);
        return Task.FromResult(settlement);
    }

    public Task<IReadOnlyList<Settlement>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        var settlements = _settlements.Values
            .Where(s => s.SpaceId == spaceId)
            .OrderByDescending(s => s.GeneratedAt)
            .ToList();
        return Task.FromResult<IReadOnlyList<Settlement>>(settlements);
    }

    public Task AddAsync(Settlement settlement, CancellationToken cancellationToken = default)
    {
        _settlements.TryAdd(settlement.Id, settlement);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Settlement settlement, CancellationToken cancellationToken = default)
    {
        _settlements.TryUpdate(settlement.Id, settlement, settlement);
        return Task.CompletedTask;
    }
}