using System.Net.Http.Json;
using BeanShare.Application.Features.Spaces.Dtos;

namespace BeanShare.Maui.Services;

public class SpacesService : ISpacesService
{
    private readonly HttpClient _httpClient;

    public SpacesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SpaceSummaryDto>> GetUserSpacesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<SpacesResponse>("/api/spaces");
            return response?.Spaces?.ToList() ?? new List<SpaceSummaryDto>();
        }
        catch
        {
            return new List<SpaceSummaryDto>();
        }
    }

    public async Task<SpaceDto?> GetSpaceByIdAsync(Guid spaceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<SpaceDto>($"/api/spaces/{spaceId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<Guid?> CreateSpaceAsync(string name, string currencyCode)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/spaces", new { Name = name, CurrencyCode = currencyCode });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CreateSpaceResponse>();
                return result?.SpaceId;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> JoinSpaceAsync(string inviteCode)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/spaces/join", new { InviteCode = inviteCode });
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private record CreateSpaceResponse(Guid SpaceId);
    private record SpacesResponse(IReadOnlyList<SpaceSummaryDto> Spaces);
}
