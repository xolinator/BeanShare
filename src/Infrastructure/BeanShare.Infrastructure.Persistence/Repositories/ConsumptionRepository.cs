using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

internal sealed class ConsumptionRepository : IConsumptionRepository
{
    private readonly BeanShareDbContext _context;

    public ConsumptionRepository(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default)
    {
        await _context.Consumptions.AddAsync(consumption, cancellationToken);
    }

    public async Task<ConsumptionEntry?> GetByIdAsync(ConsumptionEntryId id, CancellationToken cancellationToken = default)
    {
        return await _context.Consumptions
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ConsumptionEntry>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        var specification = new ConsumptionBySpaceSpecification(spaceId);
        return await _context.Consumptions
            .AsNoTracking()
            .Where(specification.Criteria)
            .OrderByDescending(c => c.ConsumedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConsumptionEntry>> GetBySpecAsync(ISpec<ConsumptionEntry> specification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(specification);

        return await _context.Consumptions
            .AsNoTracking()
            .Where(specification.Criteria)
            .OrderByDescending(c => c.ConsumedAt)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default)
    {
        _context.Consumptions.Update(consumption);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(ConsumptionEntry consumption, CancellationToken cancellationToken = default)
    {
        _context.Consumptions.Remove(consumption);
        return Task.CompletedTask;
    }
}