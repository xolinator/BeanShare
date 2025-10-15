using BeanShare.Application.Abstractions;
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

    public async Task<IEnumerable<ConsumptionEntry>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        var specification = new ConsumptionBySpaceSpecification(spaceId);
        return await _context.Consumptions
            .Where(specification.Criteria)
            .OrderByDescending(c => c.ConsumedAt)
            .ToListAsync(cancellationToken);
    }
}