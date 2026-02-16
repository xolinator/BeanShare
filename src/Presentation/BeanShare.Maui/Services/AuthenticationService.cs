using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BeanShare.Maui.Services;

/// <summary>
/// Orchestrates Keycloak OIDC authentication using PKCE flow for the MAUI app.
/// Delegates token operations to <see cref="OidcTokenHandler"/> and cryptographic
/// utilities to <see cref="PkceHelper"/>.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly OidcTokenHandler _tokenHandler;
    private readonly string _keycloakClientId;
    private readonly string _callbackUrl;
    private readonly string _authorizationEndpoint;
    private readonly string _endSessionEndpoint;
    private readonly bool _useKeycloak;
    private UserInfo? _currentUser;

    public AuthenticationService(HttpClient httpClient, ILogger<AuthenticationService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;

        var keycloakAuthority = configuration.GetValue<string>("Keycloak:Authority") ?? "http://localhost:8080/realms/beanshare";
        _keycloakClientId = configuration.GetValue<string>("Keycloak:ClientId") ?? "beanshare-mobile";
        _callbackUrl = configuration.GetValue<string>("Keycloak:RedirectUri") ?? "beanshare://callback";
        _useKeycloak = configuration.GetValue<bool>("UseKeycloak", false);

        _authorizationEndpoint = $"{keycloakAuthority}/protocol/openid-connect/auth";
        var tokenEndpoint = $"{keycloakAuthority}/protocol/openid-connect/token";
        _endSessionEndpoint = $"{keycloakAuthority}/protocol/openid-connect/logout";

        _tokenHandler = new OidcTokenHandler(httpClient, logger, _keycloakClientId, _callbackUrl, tokenEndpoint);
    }

    public Task<AuthResult> LoginAsync(string email, string password)
    {
        return LoginWithKeycloakAsync();
    }

    public Task<AuthResult> RegisterAsync(string email, string name, string password)
    {
        return LoginWithKeycloakAsync();
    }

    public Task<AuthResult> LoginWithGoogleAsync()
    {
        return LoginWithKeycloakAsync("google");
    }

    public Task<AuthResult> LoginWithFacebookAsync()
    {
        return LoginWithKeycloakAsync("facebook");
    }

    /// <summary>
    /// Initiates Keycloak PKCE authentication, optionally hinting at an identity provider.
    /// Falls back to development bypass when UseKeycloak is false in configuration.
    /// </summary>
    public async Task<AuthResult> LoginWithKeycloakAsync(string? identityProviderHint = null)
    {
        if (!_useKeycloak)
        {
            _logger.LogInformation("UseKeycloak=false - using development authentication bypass (API mock auth)");
            return await LoginWithDevBypassAsync();
        }

        try
        {
            _logger.LogInformation("Starting Keycloak PKCE authentication flow. IDP hint: {IdpHint}", identityProviderHint ?? "none");

            var codeVerifier = PkceHelper.GenerateCodeVerifier();
            var codeChallenge = PkceHelper.GenerateCodeChallenge(codeVerifier);
            var state = PkceHelper.GenerateRandomString(32);

            var authUrl = BuildAuthorizationUrl(codeChallenge, state, identityProviderHint);
            var callbackUri = new Uri(_callbackUrl);

            _logger.LogDebug("Authorization URL: {AuthUrl}", authUrl);

            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = authUrl,
                    CallbackUrl = callbackUri,
                    PrefersEphemeralWebBrowserSession = false
                });

            var code = result?.Properties.GetValueOrDefault("code");
            var returnedState = result?.Properties.GetValueOrDefault("state");

            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("Keycloak authentication returned no authorization code");
                return new AuthResult(false, null, "Authentication failed. No authorization code received.");
            }

            if (returnedState != state)
            {
                _logger.LogWarning("State mismatch in OAuth callback. Expected: {Expected}, Got: {Got}", state, returnedState);
                return new AuthResult(false, null, "Authentication failed. State mismatch.");
            }

            var authResult = await _tokenHandler.ExchangeCodeForTokensAsync(code, codeVerifier);
            if (authResult.Success)
            {
                _currentUser = authResult.User;
            }

            return authResult;
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Keycloak login was cancelled by user");
            return new AuthResult(false, null, "Authentication was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Keycloak authentication failed");
            return new AuthResult(false, null, $"Authentication failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Development-only bypass used when UseKeycloak is false in configuration.
    /// The API uses MockAuthenticationHandler (Sarah Johnson) when UseKeycloak=false.
    /// </summary>
    private async Task<AuthResult> LoginWithDevBypassAsync()
    {
        var userId = "22222222-2222-2222-2222-222222222222";
        var email = "sarah.johnson@beanshare.com";
        var name = "Sarah Johnson";

        var tcs = new TaskCompletionSource<bool>();
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await SecureStorage.Default.SetAsync("access_token", "dev-windows-token");
                await SecureStorage.Default.SetAsync("user_id", userId);
                await SecureStorage.Default.SetAsync("user_email", email);
                await SecureStorage.Default.SetAsync("user_name", name);
                await SecureStorage.Default.SetAsync("token_expires_at", DateTime.UtcNow.AddHours(24).ToString("O"));
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        await tcs.Task;

        var user = new UserInfo(Guid.Parse(userId), email, name, null);
        _currentUser = user;
        UserContextStub.SetUser(user.Id, user.Email);

        _logger.LogInformation("Development bypass login successful as {Name} ({Email})", name, email);
        return new AuthResult(true, user, null);
    }

    public async Task LogoutAsync()
    {
        _logger.LogInformation("Starting logout process");
        _currentUser = null;
        UserContextStub.ClearUser();

        _httpClient.DefaultRequestHeaders.Remove("Authorization");

        string? idToken = null;
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            idToken = await SecureStorage.Default.GetAsync("id_token");
        });

        var tcs = new TaskCompletionSource<bool>();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                SecureStorage.Default.Remove("user_id");
                SecureStorage.Default.Remove("user_email");
                SecureStorage.Default.Remove("user_name");
                SecureStorage.Default.Remove("user_picture");
                SecureStorage.Default.Remove("access_token");
                SecureStorage.Default.Remove("auth_token");
                SecureStorage.Default.Remove("refresh_token");
                SecureStorage.Default.Remove("id_token");
                SecureStorage.Default.Remove("token_expires_at");
                _logger.LogInformation("Logout completed - storage cleared");
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear storage during logout");
                tcs.SetException(ex);
            }
        });

        await tcs.Task;

        if (!string.IsNullOrEmpty(idToken))
        {
            try
            {
                var logoutUrl = $"{_endSessionEndpoint}?id_token_hint={Uri.EscapeDataString(idToken)}&post_logout_redirect_uri={Uri.EscapeDataString(_callbackUrl)}";
                await Browser.Default.OpenAsync(new Uri(logoutUrl), BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to open Keycloak logout page (non-fatal)");
            }
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        string? accessToken = null;
        string? expiresAtStr = null;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            accessToken = await SecureStorage.Default.GetAsync("access_token");
            expiresAtStr = await SecureStorage.Default.GetAsync("token_expires_at");
        });

        if (string.IsNullOrEmpty(accessToken))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(expiresAtStr) && DateTime.TryParse(expiresAtStr, out var expiresAt))
        {
            if (expiresAt < DateTime.UtcNow.AddMinutes(1))
            {
                return await _tokenHandler.TryRefreshTokensAsync();
            }
        }

        return true;
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        string? userId = null;
        string? email = null;
        string? name = null;
        string? pictureUrl = null;
        string? accessToken = null;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            userId = await SecureStorage.Default.GetAsync("user_id");
            email = await SecureStorage.Default.GetAsync("user_email");
            name = await SecureStorage.Default.GetAsync("user_name");
            pictureUrl = await SecureStorage.Default.GetAsync("user_picture");
            accessToken = await SecureStorage.Default.GetAsync("access_token");
        });

        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id))
        {
            _currentUser = new UserInfo(id, email ?? "", name ?? "", pictureUrl);
            UserContextStub.SetUser(id, email ?? "");

            if (!string.IsNullOrEmpty(accessToken))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            }

            return _currentUser;
        }

        return null;
    }

    private Uri BuildAuthorizationUrl(string codeChallenge, string state, string? identityProviderHint)
    {
        var urlBuilder = new StringBuilder(_authorizationEndpoint);
        urlBuilder.Append($"?client_id={Uri.EscapeDataString(_keycloakClientId)}");
        urlBuilder.Append($"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}");
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
}
