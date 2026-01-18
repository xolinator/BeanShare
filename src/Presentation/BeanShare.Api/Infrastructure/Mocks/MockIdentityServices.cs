using System.Security.Claims;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Enums;
using BeanShare.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using IdentityAuthService = BeanShare.Infrastructure.Identity.IAuthenticationService;

namespace BeanShare.Api.Infrastructure.Mocks;

public class MockIdentityAuthenticationService : IdentityAuthService
{
    private static readonly Dictionary<string, User> _users = new()
    {
        ["arnzrk@gmail.com"] = User.CreateWithIdAndPassword(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "arnzrk@gmail.com",
            "John Smith",
            "hashed_password",
            DateTime.UtcNow),
        ["sarah.johnson@beanshare.com"] = User.CreateWithIdAndPassword(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "sarah.johnson@beanshare.com",
            "Sarah Johnson",
            "hashed_password",
            DateTime.UtcNow),
        ["test@beanshare.com"] = User.CreateWithIdAndPassword(
            Guid.Parse("88888888-8888-8888-8888-888888888888"),
            "test@beanshare.com",
            "Test User",
            "hashed_password",
            DateTime.UtcNow)
    };

    public Task<(bool Success, User? User, string? ErrorMessage)> LoginAsync(string email, string password)
    {
        var emailLower = email.ToLowerInvariant();
        if (_users.TryGetValue(emailLower, out var user))
        {
            return Task.FromResult<(bool Success, User? User, string? ErrorMessage)>((true, user, null));
        }
        return Task.FromResult<(bool Success, User? User, string? ErrorMessage)>((false, null, "Invalid credentials"));
    }

    public Task<(bool Success, User? User, string? ErrorMessage)> RegisterAsync(string email, string name, string password)
    {
        var emailLower = email.ToLowerInvariant();
        if (_users.ContainsKey(emailLower))
        {
            return Task.FromResult<(bool Success, User? User, string? ErrorMessage)>((false, null, "Email already exists"));
        }

        var user = User.CreateWithPassword(email, name, "hashed_password", DateTime.UtcNow);
        _users[emailLower] = user;
        return Task.FromResult<(bool Success, User? User, string? ErrorMessage)>((true, user, null));
    }

    public Task<User> GetOrCreateUserAsync(ClaimsPrincipal principal, AuthenticationProvider provider)
    {
        var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "test@test.com";
        var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? "Test User";
        var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString();
        var pictureUrl = principal.FindFirst("picture")?.Value;

        var emailLower = email.ToLowerInvariant();
        if (_users.TryGetValue(emailLower, out var existingUser))
        {
            return Task.FromResult(existingUser);
        }

        var user = User.CreateWithProvider(email, name, provider, externalId, DateTime.UtcNow, pictureUrl);
        _users[emailLower] = user;
        return Task.FromResult(user);
    }

    public Task SignInAsync(HttpContext httpContext, User user, AuthenticationProperties? properties = null)
    {
        return Task.CompletedTask;
    }

    public Task SignOutAsync(HttpContext httpContext)
    {
        return Task.CompletedTask;
    }
}

public class MockJwtTokenService : IJwtTokenService
{
    public string GenerateToken(User user)
    {
        return $"mock_jwt_token_{user.Id.Value}";
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (token.StartsWith("mock_jwt_token_"))
        {
            var userIdPart = token.Replace("mock_jwt_token_", "");
            if (Guid.TryParse(userIdPart, out var userId))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, "test@test.com"),
                    new Claim(ClaimTypes.Name, "Test User")
                };
                var identity = new ClaimsIdentity(claims, "Mock");
                return new ClaimsPrincipal(identity);
            }
        }
        return null;
    }
}
