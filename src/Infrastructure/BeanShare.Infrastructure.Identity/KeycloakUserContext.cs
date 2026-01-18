using System.Security.Claims;
using System.Text.Json;
using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace BeanShare.Infrastructure.Identity;

/// <summary>
/// User context implementation for Keycloak OIDC tokens.
/// Extracts user information from Keycloak JWT claims.
/// </summary>
public sealed class KeycloakUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public KeycloakUserContext(IHttpContextAccessor httpContextAccessor)
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

            var userIdClaim = user.FindFirst("sub")?.Value
                           ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new InvalidOperationException($"User ID claim not found or invalid. Found: '{userIdClaim}'");

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

            var roles = new HashSet<string>(StringComparer.Ordinal);

            var realmAccessClaim = user.FindFirst("realm_access")?.Value;
            if (!string.IsNullOrEmpty(realmAccessClaim))
            {
                try
                {
                    using var doc = JsonDocument.Parse(realmAccessClaim);
                    if (doc.RootElement.TryGetProperty("roles", out var rolesElement) &&
                        rolesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleName = role.GetString();
                            if (!string.IsNullOrWhiteSpace(roleName))
                            {
                                roles.Add(roleName);
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                }
            }

            foreach (var claim in user.FindAll(ClaimTypes.Role))
            {
                if (!string.IsNullOrWhiteSpace(claim.Value))
                    roles.Add(claim.Value);
            }

            foreach (var claim in user.FindAll("role"))
            {
                if (!string.IsNullOrWhiteSpace(claim.Value))
                    roles.Add(claim.Value);
            }

            return roles.ToArray();
        }
    }
}
