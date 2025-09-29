using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Abstractions;

public interface ICoffeeStockRepository
{
    Task<CoffeeStock?> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default);
    Task<CoffeeStock?> GetByIdAsync(CoffeeStockId id, CancellationToken cancellationToken = default);
    Task<CoffeeStock?> GetSingleBySpecAsync<TSpec>(TSpec specification, CancellationToken cancellationToken = default)
        where TSpec : ISpec<CoffeeStock>;
    Task<IEnumerable<CoffeeStock>> GetBySpecAsync<TSpec>(TSpec specification, CancellationToken cancellationToken = default)
        where TSpec : ISpec<CoffeeStock>;
    Task AddAsync(CoffeeStock coffeeStock, CancellationToken cancellationToken = default);
    Task UpdateAsync(CoffeeStock coffeeStock, CancellationToken cancellationToken = default);
    Task<bool> ExistsForSpaceAsync(SpaceId spaceId, CancellationToken cancellationToken = default);
}