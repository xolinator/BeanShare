using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using System.Collections.Concurrent;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockConsumptionRepository : IConsumptionRepository
{
    private readonly ConcurrentDictionary<ConsumptionEntryId, ConsumptionEntry> _consumptions = new();
    private readonly IClock _clock;

    public MockConsumptionRepository(IClock clock)
    {
        _clock = clock;
        SeedData();
    }

    private void SeedData()
    {
        var spaceId = new SpaceId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var userId = new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        for (int i = 0; i < 10; i++)
        {
            var consumption = ConsumptionEntry.Create(
                spaceId,
                userId,
                CoffeeProduct.Create("Colombian Premium", "Coffee Roasters Co.", CoffeeType.Espresso),
                Weight.FromGrams(18),
                _clock.UtcNow.AddDays(-i),
                _clock);

            _consumptions.TryAdd(consumption.Id, consumption);
        }
    }

    public Task AddAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default)
    {
        _consumptions.TryAdd(consumption.Id, consumption);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ConsumptionEntry>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        var entries = _consumptions.Values
            .Where(c => c.SpaceId == spaceId)
            .OrderByDescending(c => c.ConsumedAt);
        return Task.FromResult<IEnumerable<ConsumptionEntry>>(entries);
    }

    public Task<ConsumptionEntry?> GetByIdAsync(ConsumptionEntryId id, CancellationToken cancellationToken = default)
    {
        _consumptions.TryGetValue(id, out var entry);
        return Task.FromResult(entry);
    }

    public Task<IReadOnlyList<ConsumptionEntry>> GetRecentBySpaceIdAsync(SpaceId spaceId, int limit, UserId? forUserId = null, CancellationToken ct = default)
    {
        var query = _consumptions.Values
            .Where(c => c.SpaceId == spaceId);

        if (forUserId != null)
        {
            query = query.Where(c => c.UserId == forUserId);
        }

        var entries = query
            .OrderByDescending(c => c.ConsumedAt)
            .Take(limit)
            .ToList();

        return Task.FromResult<IReadOnlyList<ConsumptionEntry>>(entries);
    }

    public Task<IReadOnlyList<ConsumptionEntry>> GetBySpecAsync(ISpec<ConsumptionEntry> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var predicate = specification.Criteria.Compile();
        var entries = _consumptions.Values
            .Where(predicate)
            .OrderByDescending(c => c.ConsumedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<ConsumptionEntry>>(entries);
    }

    public Task<(IReadOnlyList<ConsumptionEntry> Items, int TotalCount)> GetPagedBySpecAsync(ISpec<ConsumptionEntry> specification, int skip, int take, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var predicate = specification.Criteria.Compile();
        var all = _consumptions.Values
            .Where(predicate)
            .OrderByDescending(c => c.ConsumedAt)
            .ToList();

        var items = all.Skip(skip).Take(take).ToList();
        return Task.FromResult<(IReadOnlyList<ConsumptionEntry> Items, int TotalCount)>((items, all.Count));
    }

    public Task<(decimal TotalGrams, int TotalEntries, int UniqueDays)> GetSummaryBySpecAsync(ISpec<ConsumptionEntry> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        var predicate = specification.Criteria.Compile();
        var all = _consumptions.Values.Where(predicate).ToList();

        var totalGrams = all.Sum(c => c.Quantity.Grams);
        var totalEntries = all.Count;
        var uniqueDays = all.Select(c => c.ConsumedAt.Date).Distinct().Count();

        return Task.FromResult((totalGrams, totalEntries, uniqueDays));
    }

    public Task UpdateAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default)
    {
        _consumptions[consumption.Id] = consumption;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default)
    {
        _consumptions.TryRemove(consumption.Id, out _);
        return Task.CompletedTask;
    }
}