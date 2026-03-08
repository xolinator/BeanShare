using BeanShare.Application.Features.Spaces.Dtos;

namespace BeanShare.Maui.Services;

public interface ISpacesService
{
    Task<List<SpaceSummaryDto>> GetUserSpacesAsync();
    Task<SpaceDto?> GetSpaceByIdAsync(Guid spaceId);
    Task<Guid?> CreateSpaceAsync(string name, string currencyCode);
    Task<bool> JoinSpaceAsync(string inviteCode);
    Task<bool> PromoteMemberAsync(Guid spaceId, Guid userId);
    Task<bool> DemoteMemberAsync(Guid spaceId, Guid userId);
    Task<bool> RemoveMemberAsync(Guid spaceId, Guid userId);
    Task<bool> LeaveSpaceAsync(Guid spaceId);
}
