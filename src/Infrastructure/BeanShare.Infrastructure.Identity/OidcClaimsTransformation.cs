using System.Security.Claims;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using Microsoft.AspNetCore.Authentication;

namespace BeanShare.Infrastructure.Identity;

/// <summary>
/// Remaps the OIDC "sub" claim to the database user ID when they differ.
/// This happens when the database was seeded with fixed user IDs but the OIDC provider
/// assigned its own UUIDs. Falls back to email-based matching.
/// </summary>
public sealed class OidcClaimsTransformation : IClaimsTransformation
{
    private readonly IUserService _userService;

    public OidcClaimsTransformation(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        var sub = principal.FindFirst("sub")?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var oidcId))
            return principal;

        var user = await _userService.GetByIdAsync(new UserId(oidcId));
        if (user is not null)
            return principal;

        var email = principal.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(email))
            return principal;

        user = await _userService.GetByEmailAsync(email);
        if (user is null)
            return principal;

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
