using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Repositories;
using BeanShare.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BeanShare.Infrastructure.Persistence.Repositories;

public sealed class UserPresetFavoriteRepository : IUserPresetFavoriteRepository
{
    private readonly BeanShareDbContext _context;

    public UserPresetFavoriteRepository(BeanShareDbContext context)
    {
        _context = context;
    }

    public async Task<UserPresetFavorite?> GetByIdAsync(UserPresetFavoriteId id, CancellationToken ct = default)
    {
        return await _context.UserPresetFavorites
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    public async Task<IReadOnlyList<UserPresetFavorite>> GetByUserAndSpaceAsync(UserId userId, SpaceId spaceId, CancellationToken ct = default)
    {
        return await _context.UserPresetFavorites
            .Where(f => f.UserId == userId && f.SpaceId == spaceId)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<UserPresetFavorite?> GetByUserSpaceAndGlobalPresetAsync(UserId userId, SpaceId spaceId, GlobalPresetId globalPresetId, CancellationToken ct = default)
    {
        return await _context.UserPresetFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.SpaceId == spaceId && f.GlobalPresetId == globalPresetId, ct);
    }

    public async Task<UserPresetFavorite?> GetByUserSpaceAndSpacePresetAsync(UserId userId, SpaceId spaceId, PresetRecipeId presetRecipeId, CancellationToken ct = default)
    {
        return await _context.UserPresetFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.SpaceId == spaceId && f.PresetRecipeId == presetRecipeId, ct);
    }

    public async Task<int> GetMaxDisplayOrderAsync(UserId userId, SpaceId spaceId, CancellationToken ct = default)
    {
        var maxOrder = await _context.UserPresetFavorites
            .Where(f => f.UserId == userId && f.SpaceId == spaceId)
            .MaxAsync(f => (int?)f.DisplayOrder, ct);
        return maxOrder ?? 0;
    }

    public async Task AddAsync(UserPresetFavorite favorite, CancellationToken ct = default)
    {
        await _context.UserPresetFavorites.AddAsync(favorite, ct);
    }

    public Task UpdateAsync(UserPresetFavorite favorite, CancellationToken ct = default)
    {
        _context.UserPresetFavorites.Update(favorite);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(UserPresetFavoriteId id, CancellationToken ct = default)
    {
        var favorite = await GetByIdAsync(id, ct);
        if (favorite is not null)
        {
            _context.UserPresetFavorites.Remove(favorite);
        }
    }
}
