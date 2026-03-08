using BeanShare.Contracts.Presets;
using BeanShare.SharedUi.Components.Consumption.Models;

namespace BeanShare.Maui.Services;

public interface IPresetService
{
    Task<List<PresetOption>> GetQuickPresetsAsync(Guid spaceId);
    Task<bool> TogglePresetFavoriteAsync(Guid spaceId, Guid? globalPresetId, Guid? spacePresetId, bool isFavorite);
    Task<List<SpaceGlobalPresetDto>> GetSpaceGlobalPresetsAsync(Guid spaceId);
    Task<bool> ToggleGlobalPresetAsync(Guid spaceId, Guid globalPresetId, bool isEnabled);
}
