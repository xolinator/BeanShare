using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BeanShare.Maui.Services;

public class MockAuthenticationStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "22222222-2222-2222-2222-222222222222"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("sub", "22222222-2222-2222-2222-222222222222")
        }, "Mock");

        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }
}
