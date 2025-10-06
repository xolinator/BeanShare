using System.Security.Claims;
using BeanShare.Domain.Common;
using BeanShare.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace BeanShare.Infrastructure.Tests.Identity;

public class OidcUserContextTests
{
    [Fact]
    public void CurrentUserId_WithOidClaim_ShouldReturnUserId()
    {
        var userId = Guid.NewGuid();
        var httpContextAccessor = CreateHttpContextAccessor(new Claim("oid", userId.ToString()));
        var userContext = new OidcUserContext(httpContextAccessor);

        var result = userContext.CurrentUserId;

        result.Value.Should().Be(userId);
    }

    [Fact]
    public void CurrentUserId_WithNameIdentifierClaim_ShouldReturnUserId()
    {
        var userId = Guid.NewGuid();
        var httpContextAccessor = CreateHttpContextAccessor(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        var userContext = new OidcUserContext(httpContextAccessor);

        var result = userContext.CurrentUserId;

        result.Value.Should().Be(userId);
    }

    [Fact]
    public void CurrentUserId_WithSubClaim_ShouldReturnUserId()
    {
        var userId = Guid.NewGuid();
        var httpContextAccessor = CreateHttpContextAccessor(new Claim("sub", userId.ToString()));
        var userContext = new OidcUserContext(httpContextAccessor);

        var result = userContext.CurrentUserId;

        result.Value.Should().Be(userId);
    }

    [Fact]
    public void CurrentUserId_NotAuthenticated_ShouldThrow()
    {
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var userContext = new OidcUserContext(httpContextAccessor);

        var act = () => userContext.CurrentUserId;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User is not authenticated");
    }

    [Fact]
    public void Email_WithEmailClaim_ShouldReturnEmail()
    {
        var email = "user@example.com";
        var httpContextAccessor = CreateHttpContextAccessor(new Claim("email", email));
        var userContext = new OidcUserContext(httpContextAccessor);

        var result = userContext.Email;

        result.Should().Be(email);
    }

    [Fact]
    public void Email_WithPreferredUsernameClaim_ShouldReturnEmail()
    {
        var email = "user@example.com";
        var httpContextAccessor = CreateHttpContextAccessor(new Claim("preferred_username", email));
        var userContext = new OidcUserContext(httpContextAccessor);

        var result = userContext.Email;

        result.Should().Be(email);
    }

    [Fact]
    public void Email_NoClaims_ShouldReturnEmpty()
    {
        var httpContextAccessor = CreateHttpContextAccessor();
        var userContext = new OidcUserContext(httpContextAccessor);

        var result = userContext.Email;

        result.Should().BeEmpty();
    }

    [Fact]
    public void Roles_WithClaimTypesRole_ShouldReturnRoles()
    {
        var httpContextAccessor = CreateHttpContextAccessor(
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "User")
        );
        var userContext = new OidcUserContext(httpContextAccessor);

        var result = userContext.Roles;

        result.Should().BeEquivalentTo(new[] { "Admin", "User" });
    }

    [Fact]
    public void Roles_WithRoleClaim_ShouldReturnRoles()
    {
        var httpContextAccessor = CreateHttpContextAccessor(
            new Claim("role", "SpaceAdmin"),
            new Claim("role", "Member")
        );
        var userContext = new OidcUserContext(httpContextAccessor);

        var result = userContext.Roles;

        result.Should().BeEquivalentTo(new[] { "SpaceAdmin", "Member" });
    }

    [Fact]
    public void Roles_WithRolesClaim_ShouldReturnRoles()
    {
        var httpContextAccessor = CreateHttpContextAccessor(
            new Claim("roles", "GlobalAdmin"),
            new Claim("roles", "Auditor")
        );
        var userContext = new OidcUserContext(httpContextAccessor);

        var result = userContext.Roles;

        result.Should().BeEquivalentTo(new[] { "GlobalAdmin", "Auditor" });
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);
        return httpContextAccessor;
    }
}
