using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.CoffeeStock;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class CoffeeStockRepository : ICoffeeStockRepository
{
    private readonly BeanShareDbContext _context;

    public CoffeeStockRepository(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task<CoffeeStock?> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        var specification = new CoffeeStockBySpaceSpecification(spaceId);
        return await GetSingleBySpecAsync(specification, cancellationToken);
    }

    public async Task<CoffeeStock?> GetByIdAsync(CoffeeStockId id, CancellationToken cancellationToken = default)
    {
        var specification = new CoffeeStockByIdSpecification(id);
        return await GetSingleBySpecAsync(specification, cancellationToken);
    }

    public async Task<CoffeeStock?> GetSingleBySpecAsync<TSpec>(TSpec specification, CancellationToken cancellationToken = default)
        where TSpec : ISpec<CoffeeStock>
    {
        var query = _context.CoffeeStocks
            .Include(cs => cs.Purchases)
            .Include(cs => cs.StockLevels)
            .AsQueryable();

        return await query.Where(specification.Criteria).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<CoffeeStock>> GetBySpecAsync<TSpec>(TSpec specification, CancellationToken cancellationToken = default)
        where TSpec : ISpec<CoffeeStock>
    {
        var query = _context.CoffeeStocks
            .Include(cs => cs.Purchases)
            .Include(cs => cs.StockLevels)
            .AsQueryable();

        return await query.Where(specification.Criteria).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CoffeeStock coffeeStock, CancellationToken cancellationToken = default)
    {
        await _context.CoffeeStocks.AddAsync(coffeeStock, cancellationToken);
    }

    public Task UpdateAsync(CoffeeStock coffeeStock, CancellationToken cancellationToken = default)
    {
        var currentPurchaseIds = coffeeStock.Purchases.Select(p => p.Id).ToHashSet();
        var coffeeStockId = coffeeStock.Id.Value;
        foreach (var purchase in _context.ChangeTracker
            .Entries<Purchase>()
            .Where(e => e.Property<Guid>("CoffeeStockId").CurrentValue == coffeeStockId
                        && !currentPurchaseIds.Contains(e.Entity.Id))
            .Select(e => e.Entity)
            .ToList())
        {
            _context.Remove(purchase);
        }

        _context.CoffeeStocks.Update(coffeeStock);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsForSpaceAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        var specification = new CoffeeStockBySpaceSpecification(spaceId);
        return await _context.CoffeeStocks
            .AnyAsync(specification.Criteria, cancellationToken);
    }
}