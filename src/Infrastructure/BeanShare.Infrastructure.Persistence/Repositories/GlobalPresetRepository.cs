using BeanShare.Domain.Entities;
using BeanShare.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class GlobalPresetRepository : IGlobalPresetRepository
{
    private readonly BeanShareDbContext _context;

    public GlobalPresetRepository(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task<GlobalPreset?> GetByIdAsync(GlobalPresetId id, CancellationToken ct = default)
    {
        return await _context.GlobalPresets
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<GlobalPreset>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await _context.GlobalPresets
            .Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GlobalPreset>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.GlobalPresets
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task AddAsync(GlobalPreset preset, CancellationToken ct = default)
    {
        await _context.GlobalPresets.AddAsync(preset, ct);
    }

    public Task UpdateAsync(GlobalPreset preset, CancellationToken ct = default)
    {
        _context.GlobalPresets.Update(preset);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(GlobalPresetId id, CancellationToken ct = default)
    {
        return await _context.GlobalPresets.AnyAsync(p => p.Id == id, ct);
    }
}
