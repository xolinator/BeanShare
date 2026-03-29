using BeanShare.Domain.Entities;
using BeanShare.Application.Abstractions;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class SpaceGlobalPresetConfigRepository : ISpaceGlobalPresetConfigRepository
{
    private readonly BeanShareDbContext _context;

    public SpaceGlobalPresetConfigRepository(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task<SpaceGlobalPresetConfig?> GetByIdAsync(SpaceGlobalPresetConfigId id, CancellationToken ct = default)
    {
        return await _context.SpaceGlobalPresetConfigs
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<SpaceGlobalPresetConfig?> GetBySpaceAndPresetAsync(SpaceId spaceId, GlobalPresetId globalPresetId, CancellationToken ct = default)
    {
        return await _context.SpaceGlobalPresetConfigs
            .FirstOrDefaultAsync(c => c.SpaceId == spaceId && c.GlobalPresetId == globalPresetId, ct);
    }

    public async Task<IReadOnlyList<SpaceGlobalPresetConfig>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken ct = default)
    {
        return await _context.SpaceGlobalPresetConfigs
            .Where(c => c.SpaceId == spaceId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<GlobalPresetId>> GetDisabledPresetIdsForSpaceAsync(SpaceId spaceId, CancellationToken ct = default)
    {
        return await _context.SpaceGlobalPresetConfigs
            .Where(c => c.SpaceId == spaceId && !c.IsEnabled)
            .Select(c => c.GlobalPresetId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(SpaceGlobalPresetConfig config, CancellationToken ct = default)
    {
        await _context.SpaceGlobalPresetConfigs.AddAsync(config, ct);
    }

    public Task UpdateAsync(SpaceGlobalPresetConfig config, CancellationToken ct = default)
    {
        _context.SpaceGlobalPresetConfigs.Update(config);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(SpaceGlobalPresetConfigId id, CancellationToken ct = default)
    {
        var config = await GetByIdAsync(id, ct);
        if (config is not null)
        {
            _context.SpaceGlobalPresetConfigs.Remove(config);
        }
    }
}
