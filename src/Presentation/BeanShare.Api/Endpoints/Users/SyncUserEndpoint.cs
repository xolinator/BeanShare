using BeanShare.Application.Services;
using BeanShare.Contracts.Users;
using FastEndpoints;

namespace BeanShare.Api.Endpoints.Users;

/// <summary>
/// Endpoint to synchronize user from OIDC provider to local database.
/// Called by clients after authenticating with the OIDC provider.
/// </summary>
public sealed class SyncUserEndpoint : EndpointWithoutRequest<SyncUserResponse>
{
    private readonly IUserSynchronizationService _userSyncService;

    public SyncUserEndpoint(IUserSynchronizationService userSyncService)
    {
        _userSyncService = userSyncService;
    }

    public override void Configure()
    {
        Post("/api/users/sync");
        Summary(s =>
        {
            s.Summary = "Sync user from OIDC provider";
            s.Description = "Creates or updates the local user record from OIDC claims. " +
                          "Call this after successful OIDC authentication to ensure user exists in BeanShare.";
            s.Response<SyncUserResponse>(200, "User synchronized successfully");
            s.Response(401, "User is not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var principal = HttpContext.User;
        var user = await _userSyncService.SyncFromClaimsAsync(principal, ct);

        await SendOkAsync(new SyncUserResponse
        {
            UserId = user.Id.Value,
            Email = user.Email,
            Name = user.Name,
            PictureUrl = user.PictureUrl,
            Provider = user.Provider.ToString(),
            IsNewUser = user.CreatedAt == user.LastLoginAt
        }, ct);
    }
}
