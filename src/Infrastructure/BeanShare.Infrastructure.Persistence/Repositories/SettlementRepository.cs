using BeanShare.Application.Abstractions;
using BeanShare.Domain.Aggregates.Settlement;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class SettlementRepository : ISettlementRepository
{
    private readonly BeanShareDbContext _context;

    public SettlementRepository(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task<Settlement?> GetByIdAsync(SettlementId id, CancellationToken cancellationToken = default)
    {
        return await _context.Settlements
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Settlement?> GetByBillingPeriodIdAsync(BillingPeriodId billingPeriodId, CancellationToken cancellationToken = default)
    {
        return await _context.Settlements
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.BillingPeriodId == billingPeriodId, cancellationToken);
    }

    public async Task<IReadOnlyList<Settlement>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken cancellationToken = default)
    {
        return await _context.Settlements
            .Include(s => s.Lines)
            .Where(s => s.SpaceId == spaceId)
            .OrderByDescending(s => s.GeneratedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Settlement settlement, CancellationToken cancellationToken = default)
    {
        await _context.Settlements.AddAsync(settlement, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Settlement settlement, CancellationToken cancellationToken = default)
    {
        _context.Settlements.Update(settlement);
        await _context.SaveChangesAsync(cancellationToken);
    }
}