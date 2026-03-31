using System.Security.Claims;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace BeanShare.Infrastructure.Identity;

public sealed class OidcClaimsTransformation : IClaimsTransformation
{
    private readonly IUserService _userService;
    private readonly IMemoryCache _cache;

    private sealed record CachedUserResult(Guid DbUserId, SystemRole SystemRole);

    public OidcClaimsTransformation(IUserService userService, IMemoryCache cache)
    {
        _userService = userService;
        _cache = cache;
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

        var cacheKey = $"oidc_transform_{sub}";
        if (!_cache.TryGetValue(cacheKey, out CachedUserResult? cached))
        {
            var userById = await _userService.GetByIdAsync(new UserId(oidcId));

            var email = principal.FindFirst("email")?.Value;
            User? userByEmail = null;
            if (!string.IsNullOrEmpty(email))
                userByEmail = await _userService.GetByEmailAsync(email);

            var user = userByEmail ?? userById;
            if (user != null)
            {
                cached = new CachedUserResult(user.Id.Value, user.SystemRole);
                _cache.Set(cacheKey, cached, TimeSpan.FromMinutes(5));
            }
        }

        if (cached is null)
        {
            identity.AddClaim(new Claim("beanshare_transformed", "true"));
            return principal;
        }

        if (cached.DbUserId != oidcId)
        {
            var oldSub = identity.FindFirst("sub");
            if (oldSub is not null)
                identity.RemoveClaim(oldSub);
            identity.AddClaim(new Claim("sub", cached.DbUserId.ToString()));

            var oldNameId = identity.FindFirst(ClaimTypes.NameIdentifier);
            if (oldNameId is not null)
                identity.RemoveClaim(oldNameId);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, cached.DbUserId.ToString()));
        }

        if (cached.SystemRole == SystemRole.SystemAdmin && !principal.IsInRole("admin"))
            identity.AddClaim(new Claim(ClaimTypes.Role, "admin"));

        identity.AddClaim(new Claim("beanshare_transformed", "true"));
        return principal;
    }
}
