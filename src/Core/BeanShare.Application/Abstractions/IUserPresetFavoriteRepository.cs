using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Abstractions;

public interface IUserPresetFavoriteRepository
{
    Task<UserPresetFavorite?> GetByIdAsync(UserPresetFavoriteId id, CancellationToken ct = default);
    Task<IReadOnlyList<UserPresetFavorite>> GetByUserAndSpaceAsync(UserId userId, SpaceId spaceId, CancellationToken ct = default);
    Task<UserPresetFavorite?> GetByUserSpaceAndGlobalPresetAsync(UserId userId, SpaceId spaceId, GlobalPresetId globalPresetId, CancellationToken ct = default);
    Task<UserPresetFavorite?> GetByUserSpaceAndSpacePresetAsync(UserId userId, SpaceId spaceId, PresetRecipeId presetRecipeId, CancellationToken ct = default);
    Task<int> GetMaxDisplayOrderAsync(UserId userId, SpaceId spaceId, CancellationToken ct = default);
    Task AddAsync(UserPresetFavorite favorite, CancellationToken ct = default);
    Task UpdateAsync(UserPresetFavorite favorite, CancellationToken ct = default);
    Task DeleteAsync(UserPresetFavoriteId id, CancellationToken ct = default);
}
