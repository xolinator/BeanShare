using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Repositories;

public interface ISpaceGlobalPresetConfigRepository
{
    Task<SpaceGlobalPresetConfig?> GetByIdAsync(SpaceGlobalPresetConfigId id, CancellationToken ct = default);
    Task<SpaceGlobalPresetConfig?> GetBySpaceAndPresetAsync(SpaceId spaceId, GlobalPresetId globalPresetId, CancellationToken ct = default);
    Task<IReadOnlyList<SpaceGlobalPresetConfig>> GetBySpaceIdAsync(SpaceId spaceId, CancellationToken ct = default);
    Task<IReadOnlyList<GlobalPresetId>> GetDisabledPresetIdsForSpaceAsync(SpaceId spaceId, CancellationToken ct = default);
    Task AddAsync(SpaceGlobalPresetConfig config, CancellationToken ct = default);
    Task UpdateAsync(SpaceGlobalPresetConfig config, CancellationToken ct = default);
    Task DeleteAsync(SpaceGlobalPresetConfigId id, CancellationToken ct = default);
}
