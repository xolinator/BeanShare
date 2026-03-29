using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Application.Abstractions;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class PresetRecipeRepository : IPresetRecipeRepository
{
    private readonly BeanShareDbContext _context;

    public PresetRecipeRepository(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task<PresetRecipe?> GetByIdAsync(PresetRecipeId id, CancellationToken ct = default)
    {
        return await _context.PresetRecipes
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<PresetRecipe>> GetByUserIdAsync(UserId userId, CancellationToken ct = default)
    {
        return await _context.PresetRecipes
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UsageCount)
            .ThenByDescending(p => p.LastUsedAt ?? p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PresetRecipe>> GetBySpaceIdAsync(SpaceId spaceId, bool sharedOnly = false, CancellationToken ct = default)
    {
        var query = _context.PresetRecipes.Where(p => p.SpaceId == spaceId);

        if (sharedOnly)
        {
            query = query.Where(p => p.IsShared);
        }

        return await query
            .OrderByDescending(p => p.UsageCount)
            .ThenByDescending(p => p.LastUsedAt ?? p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PresetRecipe>> GetUserPresetsForSpaceAsync(UserId userId, SpaceId spaceId, CancellationToken ct = default)
    {
        return await _context.PresetRecipes
            .Where(p => p.UserId == userId && p.SpaceId == spaceId)
            .OrderByDescending(p => p.UsageCount)
            .ThenByDescending(p => p.LastUsedAt ?? p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(PresetRecipe preset, CancellationToken ct = default)
    {
        await _context.PresetRecipes.AddAsync(preset, ct);
    }

    public Task UpdateAsync(PresetRecipe preset, CancellationToken ct = default)
    {
        _context.PresetRecipes.Update(preset);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(PresetRecipeId id, CancellationToken ct = default)
    {
        var preset = await GetByIdAsync(id, ct);
        if (preset is not null)
        {
            _context.PresetRecipes.Remove(preset);
        }
    }

    public async Task<bool> ExistsAsync(PresetRecipeId id, CancellationToken ct = default)
    {
        return await _context.PresetRecipes.AnyAsync(p => p.Id == id, ct);
    }
}