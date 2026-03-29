using System.Security.Claims;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using Microsoft.AspNetCore.Authentication;

namespace BeanShare.Infrastructure.Identity;

/// <summary>
/// Remaps the OIDC "sub" claim to the database user ID when they differ,
/// and adds database-backed system role claims. This ensures that seeded admin users
/// receive the "admin" role regardless of what the OIDC provider returns.
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

        var identity = principal.Identity as ClaimsIdentity;
        if (identity is null)
            return principal;

        if (identity.HasClaim("beanshare_transformed", "true"))
            return principal;

        var sub = principal.FindFirst("sub")?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var oidcId))
            return principal;

        var userById = await _userService.GetByIdAsync(new UserId(oidcId));

        var email = principal.FindFirst("email")?.Value;
        User? userByEmail = null;
        if (!string.IsNullOrEmpty(email))
            userByEmail = await _userService.GetByEmailAsync(email);

        var user = userByEmail ?? userById;

        if (user is null)
        {
            identity.AddClaim(new Claim("beanshare_transformed", "true"));
            return principal;
        }

        if (user.Id.Value != oidcId)
        {
            var oldSub = identity.FindFirst("sub");
            if (oldSub is not null)
                identity.RemoveClaim(oldSub);
            identity.AddClaim(new Claim("sub", user.Id.Value.ToString()));

            var oldNameId = identity.FindFirst(ClaimTypes.NameIdentifier);
            if (oldNameId is not null)
                identity.RemoveClaim(oldNameId);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.Value.ToString()));
        }

        // Add database-backed admin role if the OIDC token doesn't already include it
        if (user.SystemRole == SystemRole.SystemAdmin && !principal.IsInRole("admin"))
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));

        identity.AddClaim(new Claim("beanshare_transformed", "true"));
        return principal;
    }
}
