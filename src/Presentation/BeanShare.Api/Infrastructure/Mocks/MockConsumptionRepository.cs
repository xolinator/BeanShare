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
}