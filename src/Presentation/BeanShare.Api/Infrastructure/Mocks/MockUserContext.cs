using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using Microsoft.AspNetCore.Http;

namespace BeanShare.Api.Infrastructure.Mocks;

public sealed class MockUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MockUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserId CurrentUserId
    {
        get
        {
            // Check for X-Test-UserId header to allow multi-user testing
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.Headers.TryGetValue("X-Test-UserId", out var userIdHeader) == true
                && Guid.TryParse(userIdHeader, out var userId))
            {
                return new UserId(userId);
            }

            // Default test user (matches the user that creates the space)
            return new UserId(Guid.Parse("22222222-2222-2222-2222-222222222222"));
        }
    }

    public string Email
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.Headers.TryGetValue("X-Test-Email", out var emailHeader) == true)
            {
                return emailHeader.ToString();
            }

            return "test@coffeespace.local";
        }
    }

    public IReadOnlyCollection<string> Roles { get; } = new[] { "User" };
}