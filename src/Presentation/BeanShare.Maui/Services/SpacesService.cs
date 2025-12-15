using System.Net.Http.Json;
using BeanShare.Application.Features.Spaces.Dtos;
using Microsoft.Extensions.Logging;

namespace BeanShare.Maui.Services;

public class SpacesService : ISpacesService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SpacesService> _logger;

    public SpacesService(HttpClient httpClient, ILogger<SpacesService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<SpaceSummaryDto>> GetUserSpacesAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<SpacesResponse>("/api/spaces");
            return response?.Spaces?.ToList() ?? new List<SpaceSummaryDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch user spaces - network error");
            return new List<SpaceSummaryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching user spaces");
            return new List<SpaceSummaryDto>();
        }
    }

    public async Task<SpaceDto?> GetSpaceByIdAsync(Guid spaceId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<SpaceByIdResponse>($"/api/spaces/{spaceId}");
            if (response == null) return null;

            return new SpaceDto
            {
                Id = response.Id,
                Name = response.Name,
                CurrencyCode = "CZK", // Default - API doesn't return this
                InviteCode = response.InviteCode,
                IsActive = true, // Default - API doesn't return this
                CreatedBy = Guid.Empty, // Default - API doesn't return this
                CreatedAt = response.CreatedAt,
                MemberCount = response.MemberCount,
                Members = response.Members.Select(m => new MembershipDto
                {
                    UserId = m.UserId,
                    Email = m.Email,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt,
                    UserName = m.UserName
                }).ToList()
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch space {SpaceId} - network error", spaceId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching space {SpaceId}", spaceId);
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
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to create space '{SpaceName}' - network error", name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating space '{SpaceName}'", name);
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
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to join space with invite code - network error");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while joining space");
            return false;
        }
    }

    public async Task<bool> PromoteMemberAsync(Guid spaceId, Guid userId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/api/spaces/{spaceId}/members/{userId}/promote",
                new { SpaceId = spaceId, UserId = userId });
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to promote member {UserId} in space {SpaceId} - network error", userId, spaceId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while promoting member {UserId} in space {SpaceId}", userId, spaceId);
            return false;
        }
    }

    public async Task<bool> DemoteMemberAsync(Guid spaceId, Guid userId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/api/spaces/{spaceId}/members/{userId}/demote",
                new { SpaceId = spaceId, UserId = userId });
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to demote member {UserId} in space {SpaceId} - network error", userId, spaceId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while demoting member {UserId} in space {SpaceId}", userId, spaceId);
            return false;
        }
    }

    public async Task<bool> RemoveMemberAsync(Guid spaceId, Guid userId)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/api/spaces/{spaceId}/members/{userId}/remove",
                new { SpaceId = spaceId, UserId = userId });
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to remove member {UserId} from space {SpaceId} - network error", userId, spaceId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while removing member {UserId} from space {SpaceId}", userId, spaceId);
            return false;
        }
    }

    private record CreateSpaceResponse(Guid SpaceId);
    private record SpacesResponse(IReadOnlyList<SpaceSummaryDto> Spaces);

    private record SpaceByIdResponse(
        Guid Id,
        string Name,
        string InviteCode,
        int MemberCount,
        DateTime CreatedAt,
        List<SpaceMemberResponse> Members
    );

    private record SpaceMemberResponse(
        Guid UserId,
        string Email,
        string Role,
        DateTime JoinedAt,
        string UserName = "Unknown User"
    );
}
