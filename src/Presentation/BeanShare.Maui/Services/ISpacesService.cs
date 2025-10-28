using BeanShare.Application.Features.Spaces.Dtos;

namespace BeanShare.Maui.Services;

public interface ISpacesService
{
    Task<List<SpaceSummaryDto>> GetUserSpacesAsync();
    Task<SpaceDto?> GetSpaceByIdAsync(Guid spaceId);
    Task<Guid?> CreateSpaceAsync(string name, string currencyCode);
    Task<bool> JoinSpaceAsync(string inviteCode);
}
