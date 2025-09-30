using BeanShare.Application.Abstractions;
using BeanShare.Domain.Entities;
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
}