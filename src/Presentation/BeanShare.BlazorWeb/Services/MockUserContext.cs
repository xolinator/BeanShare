using System.Security.Claims;
using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;

namespace BeanShare.BlazorWeb.Services;

/// <summary>
/// User context that reads from HttpContext authentication
/// Falls back to default user when not authenticated (for backward compatibility)
/// </summary>
public class HttpUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserId CurrentUserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("sub")?.Value
                    ?? user.FindFirst("UserId")?.Value;

                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    return new UserId(userId);
                }
            }

            // Fallback - John Smith for backward compatibility with seeded data
            return new UserId(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        }
    }

    public string Email
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                return user.FindFirst(ClaimTypes.Email)?.Value
                    ?? user.FindFirst("email")?.Value
                    ?? "john.smith@beanshare.com";
            }
            return "john.smith@beanshare.com";
        }
    }

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
                if (roles.Count > 0) return roles;
            }
            return new[] { "User" };
        }
    }
}
