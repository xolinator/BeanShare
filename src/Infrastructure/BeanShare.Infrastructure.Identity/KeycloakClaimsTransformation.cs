using System.Security.Claims;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using Microsoft.AspNetCore.Authentication;

namespace BeanShare.Infrastructure.Identity;

/// <summary>
/// Remaps the Keycloak "sub" claim to the database user ID when they differ.
/// This happens when the database was seeded with fixed user IDs but Keycloak
/// assigned its own UUIDs. Falls back to email-based matching.
/// </summary>
public sealed class KeycloakClaimsTransformation : IClaimsTransformation
{
    private readonly IUserService _userService;

    public KeycloakClaimsTransformation(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        var sub = principal.FindFirst("sub")?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var keycloakId))
            return principal;

        // Check if user exists by Keycloak UUID
        var user = await _userService.GetByIdAsync(new UserId(keycloakId));
        if (user is not null)
            return principal; // IDs match, no remapping needed

        // Try email-based fallback
        var email = principal.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(email))
            return principal;

        user = await _userService.GetByEmailAsync(email);
        if (user is null)
            return principal; // No matching user, sync service will create one

        // Remap: replace sub claim with the database user's ID
        var identity = principal.Identity as ClaimsIdentity;
        if (identity is null)
            return principal;

        var oldSub = identity.FindFirst("sub");
        if (oldSub is not null)
            identity.RemoveClaim(oldSub);
        identity.AddClaim(new Claim("sub", user.Id.Value.ToString()));

        var oldNameId = identity.FindFirst(ClaimTypes.NameIdentifier);
        if (oldNameId is not null)
            identity.RemoveClaim(oldNameId);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.Value.ToString()));

        return principal;
    }
}
