using BeanShare.Domain.Entities;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Repositories;

public interface IPresetRecipeRepository
{
    Task<PresetRecipe?> GetByIdAsync(PresetRecipeId id, CancellationToken ct = default);
    Task<IReadOnlyList<PresetRecipe>> GetByUserIdAsync(UserId userId, CancellationToken ct = default);
    Task<IReadOnlyList<PresetRecipe>> GetBySpaceIdAsync(SpaceId spaceId, bool sharedOnly = false, CancellationToken ct = default);
    Task<IReadOnlyList<PresetRecipe>> GetUserPresetsForSpaceAsync(UserId userId, SpaceId spaceId, CancellationToken ct = default);
    Task AddAsync(PresetRecipe preset, CancellationToken ct = default);
    Task UpdateAsync(PresetRecipe preset, CancellationToken ct = default);
    Task DeleteAsync(PresetRecipeId id, CancellationToken ct = default);
    Task<bool> ExistsAsync(PresetRecipeId id, CancellationToken ct = default);
}