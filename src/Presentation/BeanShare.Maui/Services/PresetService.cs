using System.Net.Http.Json;
using BeanShare.Contracts.Presets;
using BeanShare.SharedUi.Components.Consumption.Models;

namespace BeanShare.Maui.Services;

public class PresetService : IPresetService
{
    private readonly HttpClient _httpClient;
    private readonly ApiResponseCache _cache;

    public PresetService(HttpClient httpClient, ApiResponseCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<List<PresetOption>> GetQuickPresetsAsync(Guid spaceId)
    {
        var cacheKey = $"presets:{spaceId}";
        var cached = _cache.Get<List<PresetOption>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<GetQuickPresetsResponse>($"/api/spaces/{spaceId}/quick-presets");
            if (response?.Presets == null)
            {
                return new List<PresetOption>();
            }

            var presets = response.Presets.Select(p => new PresetOption
            {
                GlobalPresetId = p.GlobalPresetId,
                SpacePresetId = p.SpacePresetId,
                Name = p.Name,
                CoffeeType = p.CoffeeType,
                Preparation = p.Preparation,
                Grams = (int)p.DefaultGrams,
                Description = p.Description,
                IsFavorite = p.IsFavorite,
                IsGlobal = p.IsGlobal,
                DisplayOrder = p.DisplayOrder
            }).ToList();

            _cache.Set(cacheKey, presets, TimeSpan.FromSeconds(45));
            return presets;
        }
        catch
        {
            return new List<PresetOption>();
        }
    }

    public async Task<PresetDto?> GetPresetByIdAsync(Guid presetId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PresetDto>($"/api/presets/{presetId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> TogglePresetFavoriteAsync(Guid spaceId, Guid? globalPresetId, Guid? spacePresetId, bool isFavorite)
    {
        try
        {
            var request = new TogglePresetFavoriteRequest
            {
                GlobalPresetId = globalPresetId,
                SpacePresetId = spacePresetId,
                IsFavorite = isFavorite
            };

            var response = await _httpClient.PostAsJsonAsync($"/api/spaces/{spaceId}/preset-favorites/toggle", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<SpaceGlobalPresetDto>> GetSpaceGlobalPresetsAsync(Guid spaceId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<GetSpaceGlobalPresetsResponse>($"/api/spaces/{spaceId}/global-presets");
            return response?.Presets?.ToList() ?? new List<SpaceGlobalPresetDto>();
        }
        catch
        {
            return new List<SpaceGlobalPresetDto>();
        }
    }

    public async Task<bool> ToggleGlobalPresetAsync(Guid spaceId, Guid globalPresetId, bool isEnabled)
    {
        try
        {
            var request = new ToggleGlobalPresetRequest { IsEnabled = isEnabled };
            var response = await _httpClient.PutAsJsonAsync($"/api/spaces/{spaceId}/global-presets/{globalPresetId}/toggle", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
