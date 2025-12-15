using BeanShare.Domain.Entities;

namespace BeanShare.Domain.Repositories;

public interface IGlobalPresetRepository
{
    Task<GlobalPreset?> GetByIdAsync(GlobalPresetId id, CancellationToken ct = default);
    Task<IReadOnlyList<GlobalPreset>> GetAllActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<GlobalPreset>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(GlobalPreset preset, CancellationToken ct = default);
    Task UpdateAsync(GlobalPreset preset, CancellationToken ct = default);
    Task<bool> ExistsAsync(GlobalPresetId id, CancellationToken ct = default);
}
