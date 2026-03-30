using System.Security.Claims;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using Microsoft.AspNetCore.Authentication;

namespace BeanShare.Infrastructure.Identity;

/// <summary>
/// Remaps the OIDC "sub" claim to the database user ID when they differ,
/// and adds database-backed system role claims. This supports both GUID-based
/// and opaque provider subject identifiers.
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

        if (string.IsNullOrWhiteSpace(sub))
            return principal;

        var identityProvider = principal.FindFirst("identity_provider")?.Value ?? "oidc";
        var provider = identityProvider.ToLowerInvariant() switch
        {
            "google" => AuthenticationProvider.Google,
            "facebook" => AuthenticationProvider.Facebook,
            _ => AuthenticationProvider.Oidc
        };

        var userByProvider = await _userService.GetByProviderUserIdAsync(provider, sub);

        var email = principal.FindFirst("email")?.Value;
        User? userByEmail = null;
        if (!string.IsNullOrEmpty(email))
            userByEmail = await _userService.GetByEmailAsync(email);

        var user = userByProvider ?? userByEmail;

        if (user is null)
        {
            identity.AddClaim(new Claim("beanshare_transformed", "true"));
            return principal;
        }

        if (!string.Equals(sub, user.Id.Value.ToString(), StringComparison.Ordinal))
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
