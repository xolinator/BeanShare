using System.Web;

namespace BeanShare.Tests.Unit.Services;

public sealed class AuthorizationUrlBuilderTests
{
    private const string AuthorizationEndpoint = "http://localhost:8080/realms/beanshare/protocol/openid-connect/auth";
    private const string ClientId = "beanshare-mobile";
    private const string CallbackUrl = "beanshare://callback";

    private static Uri BuildAuthorizationUrl(string codeChallenge, string state, string? identityProviderHint = null)
    {
        var urlBuilder = new System.Text.StringBuilder(AuthorizationEndpoint);
        urlBuilder.Append($"?client_id={Uri.EscapeDataString(ClientId)}");
        urlBuilder.Append($"&redirect_uri={Uri.EscapeDataString(CallbackUrl)}");
        urlBuilder.Append("&response_type=code");
        urlBuilder.Append("&scope=openid%20profile%20email");
        urlBuilder.Append($"&code_challenge={Uri.EscapeDataString(codeChallenge)}");
        urlBuilder.Append("&code_challenge_method=S256");
        urlBuilder.Append($"&state={Uri.EscapeDataString(state)}");

        if (!string.IsNullOrEmpty(identityProviderHint))
        {
            urlBuilder.Append($"&kc_idp_hint={Uri.EscapeDataString(identityProviderHint)}");
        }

        return new Uri(urlBuilder.ToString());
    }

    [Fact]
    public void BuildAuthorizationUrl_ShouldIncludeClientId()
    {
        var url = BuildAuthorizationUrl("challenge", "state123");
        var query = HttpUtility.ParseQueryString(url.Query);

        query["client_id"].Should().Be(ClientId);
    }

    [Fact]
    public void BuildAuthorizationUrl_ShouldIncludeRedirectUri()
    {
        var url = BuildAuthorizationUrl("challenge", "state123");
        var query = HttpUtility.ParseQueryString(url.Query);

        query["redirect_uri"].Should().Be(CallbackUrl);
    }

    [Fact]
    public void BuildAuthorizationUrl_ShouldUseResponseTypeCode()
    {
        var url = BuildAuthorizationUrl("challenge", "state123");
        var query = HttpUtility.ParseQueryString(url.Query);

        query["response_type"].Should().Be("code");
    }

    [Fact]
    public void BuildAuthorizationUrl_ShouldRequestOpenIdProfileEmailScopes()
    {
        var url = BuildAuthorizationUrl("challenge", "state123");
        var query = HttpUtility.ParseQueryString(url.Query);

        query["scope"].Should().Be("openid profile email");
    }

    [Fact]
    public void BuildAuthorizationUrl_ShouldIncludeCodeChallenge()
    {
        var codeChallenge = "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM";
        var url = BuildAuthorizationUrl(codeChallenge, "state123");
        var query = HttpUtility.ParseQueryString(url.Query);

        query["code_challenge"].Should().Be(codeChallenge);
    }

    [Fact]
    public void BuildAuthorizationUrl_ShouldUseS256ChallengeMethod()
    {
        var url = BuildAuthorizationUrl("challenge", "state123");
        var query = HttpUtility.ParseQueryString(url.Query);

        query["code_challenge_method"].Should().Be("S256");
    }

    [Fact]
    public void BuildAuthorizationUrl_ShouldIncludeState()
    {
        var state = "random_state_value_abc123";
        var url = BuildAuthorizationUrl("challenge", state);
        var query = HttpUtility.ParseQueryString(url.Query);

        query["state"].Should().Be(state);
    }

    [Fact]
    public void BuildAuthorizationUrl_WithoutIdpHint_ShouldNotIncludeKcIdpHint()
    {
        var url = BuildAuthorizationUrl("challenge", "state123");
        var query = HttpUtility.ParseQueryString(url.Query);

        query["kc_idp_hint"].Should().BeNull();
    }

    [Fact]
    public void BuildAuthorizationUrl_WithGoogleHint_ShouldIncludeKcIdpHint()
    {
        var url = BuildAuthorizationUrl("challenge", "state123", "google");
        var query = HttpUtility.ParseQueryString(url.Query);

        query["kc_idp_hint"].Should().Be("google");
    }

    [Fact]
    public void BuildAuthorizationUrl_WithFacebookHint_ShouldIncludeKcIdpHint()
    {
        var url = BuildAuthorizationUrl("challenge", "state123", "facebook");
        var query = HttpUtility.ParseQueryString(url.Query);

        query["kc_idp_hint"].Should().Be("facebook");
    }

    [Fact]
    public void BuildAuthorizationUrl_ShouldPointToCorrectEndpoint()
    {
        var url = BuildAuthorizationUrl("challenge", "state123");

        url.Scheme.Should().Be("http");
        url.Host.Should().Be("localhost");
        url.Port.Should().Be(8080);
        url.AbsolutePath.Should().Be("/realms/beanshare/protocol/openid-connect/auth");
    }

    [Fact]
    public void BuildAuthorizationUrl_ShouldProperlyEncodeSpecialCharacters()
    {
        // Code challenges may contain Base64URL characters
        var codeChallenge = "abc-def_ghi";
        var url = BuildAuthorizationUrl(codeChallenge, "state+with+special");
        var query = HttpUtility.ParseQueryString(url.Query);

        query["code_challenge"].Should().Be(codeChallenge);
        query["state"].Should().Be("state+with+special");
    }

    [Fact]
    public void PkceFlow_EndToEnd_ShouldProduceValidUrl()
    {
        // Simulate full PKCE flow: generate verifier -> challenge -> URL
        var verifier = BeanShare.Maui.Services.PkceHelper.GenerateCodeVerifier();
        var challenge = BeanShare.Maui.Services.PkceHelper.GenerateCodeChallenge(verifier);
        var state = BeanShare.Maui.Services.PkceHelper.GenerateRandomString(32);

        var url = BuildAuthorizationUrl(challenge, state);
        var query = HttpUtility.ParseQueryString(url.Query);

        // All required OAuth2 PKCE parameters should be present
        query["client_id"].Should().NotBeNullOrEmpty();
        query["redirect_uri"].Should().NotBeNullOrEmpty();
        query["response_type"].Should().Be("code");
        query["scope"].Should().Contain("openid");
        query["code_challenge"].Should().NotBeNullOrEmpty();
        query["code_challenge_method"].Should().Be("S256");
        query["state"].Should().NotBeNullOrEmpty();

        // Challenge and state should be the values we generated
        query["code_challenge"].Should().Be(challenge);
        query["state"].Should().Be(state);
    }
}
