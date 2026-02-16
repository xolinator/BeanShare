using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace BeanShare.Tests.Unit.Services;

public sealed class OidcTokenParsingTests
{
    private static string CreateTestJwt(IDictionary<string, object> claims)
    {
        var key = new SymmetricSecurityKey(new byte[32]);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    [Fact]
    public void ReadJwtToken_ShouldExtractSubClaim()
    {
        var userId = Guid.NewGuid();
        var jwt = CreateTestJwt(new Dictionary<string, object>
        {
            ["sub"] = userId.ToString(),
            ["email"] = "test@example.com",
            ["name"] = "Test User"
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        var sub = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        sub.Should().NotBeNull();
        Guid.TryParse(sub, out var parsedId).Should().BeTrue();
        parsedId.Should().Be(userId);
    }

    [Fact]
    public void ReadJwtToken_ShouldExtractEmailClaim()
    {
        var jwt = CreateTestJwt(new Dictionary<string, object>
        {
            ["sub"] = Guid.NewGuid().ToString(),
            ["email"] = "alice@beanshare.com",
            ["name"] = "Alice"
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        var email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

        email.Should().Be("alice@beanshare.com");
    }

    [Fact]
    public void ReadJwtToken_ShouldExtractNameClaim()
    {
        var jwt = CreateTestJwt(new Dictionary<string, object>
        {
            ["sub"] = Guid.NewGuid().ToString(),
            ["email"] = "bob@beanshare.com",
            ["name"] = "Bob Smith"
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        var name = token.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

        name.Should().Be("Bob Smith");
    }

    [Fact]
    public void ReadJwtToken_ShouldFallbackToPreferredUsername_WhenNameMissing()
    {
        var jwt = CreateTestJwt(new Dictionary<string, object>
        {
            ["sub"] = Guid.NewGuid().ToString(),
            ["email"] = "bob@beanshare.com",
            ["preferred_username"] = "bob123"
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        var name = token.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                ?? token.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

        name.Should().Be("bob123");
    }

    [Fact]
    public void ReadJwtToken_ShouldHandleMissingOptionalClaims()
    {
        var jwt = CreateTestJwt(new Dictionary<string, object>
        {
            ["sub"] = Guid.NewGuid().ToString()
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        var email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var name = token.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
        var picture = token.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

        email.Should().BeNull();
        name.Should().BeNull();
        picture.Should().BeNull();
    }

    [Fact]
    public void ReadJwtToken_ShouldRejectInvalidSubAsGuid()
    {
        var jwt = CreateTestJwt(new Dictionary<string, object>
        {
            ["sub"] = "not-a-guid",
            ["email"] = "test@example.com"
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        var sub = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

        Guid.TryParse(sub, out _).Should().BeFalse("non-GUID sub should fail parsing");
    }

    [Fact]
    public void ReadJwtToken_ShouldExtractPictureClaim()
    {
        var pictureUrl = "https://example.com/avatar.jpg";
        var jwt = CreateTestJwt(new Dictionary<string, object>
        {
            ["sub"] = Guid.NewGuid().ToString(),
            ["email"] = "user@example.com",
            ["picture"] = pictureUrl
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        var picture = token.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

        picture.Should().Be(pictureUrl);
    }

    [Fact]
    public void ReadJwtToken_ShouldParseExpiration()
    {
        var jwt = CreateTestJwt(new Dictionary<string, object>
        {
            ["sub"] = Guid.NewGuid().ToString()
        });

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        token.ValidTo.Should().BeAfter(DateTime.UtcNow, "token should not be expired");
    }
}
