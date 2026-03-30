using System.Security.Claims;
using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace BeanShare.Infrastructure.Identity;

public sealed class OidcUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OidcUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserId CurrentUserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException("User is not authenticated");

            // Claim chain: "oid" → ClaimTypes.NameIdentifier → "sub"
            var userIdClaim = user.FindFirst("oid")?.Value
                           ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new InvalidOperationException("User ID claim not found or invalid");

            return new UserId(userId);
        }
    }

    public string Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return string.Empty;

            // Claim chain: "email" → "preferred_username" → string.Empty
            return user.FindFirst("email")?.Value
                ?? user.FindFirst("preferred_username")?.Value
                ?? string.Empty;
        }
    }

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return Array.Empty<string>();

            var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value)
                            .Concat(user.FindAll("role").Select(c => c.Value))
                            .Concat(user.FindAll("roles").Select(c => c.Value))
                            .Concat(user.FindAll("groups").Select(c => c.Value))
                            .Where(v => !string.IsNullOrWhiteSpace(v))
                            .Distinct(StringComparer.Ordinal)
                            .ToArray();
            return roles;
        }
    }
}
