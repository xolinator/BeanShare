using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using System.Collections.Concurrent;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockCoffeeStockRepository : ICoffeeStockRepository
{
    private readonly ConcurrentDictionary<CoffeeStockId, CoffeeStock> _stocks = new();
    private readonly IClock _clock;

    public MockCoffeeStockRepository(IClock clock)
    {
        _clock = clock;
        SeedData();
    }

    private void SeedData()
    {
        var spaceId = new SpaceId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var userId = new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        var product = CoffeeProduct.Create("Colombian Premium", "Coffee Roasters Co.", CoffeeType.Espresso);
        var stock = CoffeeStock.Create(spaceId, _clock);

        stock.AddPurchase(
            product,
            Weight.FromGrams(1000),
            Money.Create(25.99m, "USD"),
            "Local Coffee Shop",
            userId,
            _clock.UtcNow.AddDays(-7),
            _clock);

        _stocks.TryAdd(stock.Id, stock);
    }

    public Task<CoffeeStock?> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        var stock = _stocks.Values.FirstOrDefault(s => s.SpaceId == spaceId);
        return Task.FromResult(stock);
    }

    public Task<CoffeeStock?> GetByIdAsync(CoffeeStockId id, CancellationToken cancellationToken = default)
    {
        _stocks.TryGetValue(id, out var stock);
        return Task.FromResult(stock);
    }

    public Task<CoffeeStock?> GetSingleBySpecAsync<TSpec>(TSpec specification, CancellationToken cancellationToken = default)
        where TSpec : ISpec<CoffeeStock>
    {
        var compiledCriteria = specification.Criteria.Compile();
        var stock = _stocks.Values.FirstOrDefault(compiledCriteria);
        return Task.FromResult(stock);
    }

    public Task<IEnumerable<CoffeeStock>> GetBySpecAsync<TSpec>(TSpec specification, CancellationToken cancellationToken = default)
        where TSpec : ISpec<CoffeeStock>
    {
        var compiledCriteria = specification.Criteria.Compile();
        var stocks = _stocks.Values.Where(compiledCriteria);
        return Task.FromResult<IEnumerable<CoffeeStock>>(stocks);
    }

    public Task AddAsync(CoffeeStock coffeeStock, CancellationToken cancellationToken = default)
    {
        _stocks.TryAdd(coffeeStock.Id, coffeeStock);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(CoffeeStock coffeeStock, CancellationToken cancellationToken = default)
    {
        _stocks.TryUpdate(coffeeStock.Id, coffeeStock, coffeeStock);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsForSpaceAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        var exists = _stocks.Values.Any(s => s.SpaceId == spaceId);
        return Task.FromResult(exists);
    }
}