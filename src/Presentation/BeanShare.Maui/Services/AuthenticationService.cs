using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BeanShare.Maui.Services;

/// <summary>
/// Orchestrates OIDC authentication using PKCE flow for the MAUI app.
/// Delegates token operations to <see cref="OidcTokenHandler"/> and cryptographic
/// utilities to <see cref="PkceHelper"/>.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly OidcTokenHandler _tokenHandler;
    private readonly string _oidcClientId;
    private readonly string _callbackUrl;
    private readonly string _authorizationEndpoint;
    private readonly string _registrationEndpoint;
    private readonly string _endSessionEndpoint;
    private readonly string? _idpHintParam;
    private readonly bool _useOidc;
    private UserInfo? _currentUser;

    public AuthenticationService(HttpClient httpClient, ILogger<AuthenticationService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;

        var oidcAuthority = configuration.GetValue<string>("Oidc:Authority") ?? "http://localhost:8080/realms/beanshare";
        _oidcClientId = configuration.GetValue<string>("Oidc:ClientId") ?? "beanshare-mobile";
        _callbackUrl = configuration.GetValue<string>("Oidc:RedirectUri") ?? "beanshare://callback";
        _useOidc = configuration.GetValue<bool>("UseOidc", false);
        _idpHintParam = configuration.GetValue<string>("Oidc:IdentityProviderHintParam");

        // Android emulator uses 10.0.2.2 to reach the host machine's localhost
#if ANDROID
        if (oidcAuthority.Contains("localhost") || oidcAuthority.Contains("127.0.0.1"))
        {
            oidcAuthority = oidcAuthority.Replace("localhost", "10.0.2.2").Replace("127.0.0.1", "10.0.2.2");
            logger.LogInformation("Android emulator detected - OIDC authority remapped to: {Authority}", oidcAuthority);
        }
#endif

        // Use explicit endpoint overrides if configured, otherwise derive from authority using common OIDC provider conventions
        _authorizationEndpoint = configuration.GetValue<string>("Oidc:AuthorizationEndpoint");
        if (string.IsNullOrEmpty(_authorizationEndpoint))
            _authorizationEndpoint = $"{oidcAuthority}/protocol/openid-connect/auth";

        _registrationEndpoint = configuration.GetValue<string>("Oidc:RegistrationEndpoint");
        if (string.IsNullOrEmpty(_registrationEndpoint))
            _registrationEndpoint = $"{oidcAuthority}/protocol/openid-connect/registrations";

        var tokenEndpoint = configuration.GetValue<string>("Oidc:TokenEndpoint");
        if (string.IsNullOrEmpty(tokenEndpoint))
            tokenEndpoint = $"{oidcAuthority}/protocol/openid-connect/token";

        _endSessionEndpoint = configuration.GetValue<string>("Oidc:EndSessionEndpoint");
        if (string.IsNullOrEmpty(_endSessionEndpoint))
            _endSessionEndpoint = $"{oidcAuthority}/protocol/openid-connect/logout";

        _tokenHandler = new OidcTokenHandler(httpClient, logger, _oidcClientId, _callbackUrl, tokenEndpoint);
    }

    public Task<AuthResult> LoginAsync(string email, string password)
    {
        return LoginWithOidcAsync();
    }

    public Task<AuthResult> RegisterAsync(string email, string name, string password)
    {
        return RegisterWithOidcAsync();
    }

    /// <summary>
    /// Opens the OIDC provider's self-registration page using PKCE flow.
    /// Uses the registration endpoint instead of the authorization endpoint to show the registration form.
    /// </summary>
    public async Task<AuthResult> RegisterWithOidcAsync()
    {
        if (!_useOidc)
        {
            _logger.LogInformation("UseOidc=false - using development authentication bypass for registration");
            return await LoginWithDevBypassAsync();
        }

        try
        {
            _logger.LogInformation("Starting OIDC PKCE registration flow");

            var codeVerifier = PkceHelper.GenerateCodeVerifier();
            var codeChallenge = PkceHelper.GenerateCodeChallenge(codeVerifier);
            var state = PkceHelper.GenerateRandomString(32);

            var registrationUrl = BuildRegistrationUrl(codeChallenge, state);
            var callbackUri = new Uri(_callbackUrl);

            _logger.LogDebug("Registration URL: {RegUrl}", registrationUrl);

            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = registrationUrl,
                    CallbackUrl = callbackUri,
                    PrefersEphemeralWebBrowserSession = false
                });

            var code = result?.Properties.GetValueOrDefault("code");
            var returnedState = result?.Properties.GetValueOrDefault("state");

            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("OIDC registration returned no authorization code");
                return new AuthResult(false, null, "Registration failed. No authorization code received.");
            }

            if (returnedState != state)
            {
                _logger.LogWarning("State mismatch in registration callback");
                return new AuthResult(false, null, "Registration failed. State mismatch.");
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
            _logger.LogInformation("OIDC registration was cancelled by user");
            return new AuthResult(false, null, "Registration was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OIDC registration failed");
            return new AuthResult(false, null, $"Registration failed: {ex.Message}");
        }
    }

    public Task<AuthResult> LoginWithGoogleAsync()
    {
        return LoginWithOidcAsync("google");
    }

    public Task<AuthResult> LoginWithFacebookAsync()
    {
        return LoginWithOidcAsync("facebook");
    }

    /// <summary>
    /// Initiates OIDC PKCE authentication, optionally hinting at an identity provider.
    /// Falls back to development bypass when UseOidc is false in configuration.
    /// </summary>
    public async Task<AuthResult> LoginWithOidcAsync(string? identityProviderHint = null)
    {
        if (!_useOidc)
        {
            _logger.LogInformation("UseOidc=false - using development authentication bypass (API mock auth)");
            return await LoginWithDevBypassAsync();
        }

        try
        {
            _logger.LogInformation("Starting OIDC PKCE authentication flow. IDP hint: {IdpHint}", identityProviderHint ?? "none");

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
                _logger.LogWarning("OIDC authentication returned no authorization code");
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
            _logger.LogInformation("OIDC login was cancelled by user");
            return new AuthResult(false, null, "Authentication was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OIDC authentication failed");
            return new AuthResult(false, null, $"Authentication failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Development-only bypass used when UseOidc is false in configuration.
    /// The API uses MockAuthenticationHandler (Sarah Johnson) when UseOidc=false.
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
                _logger.LogWarning(ex, "Failed to open OIDC logout page (non-fatal)");
            }
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        return await _tokenHandler.TryRefreshTokensAsync();
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
        urlBuilder.Append($"?client_id={Uri.EscapeDataString(_oidcClientId)}");
        urlBuilder.Append($"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}");
        urlBuilder.Append("&response_type=code");
        urlBuilder.Append("&scope=openid%20profile%20email");
        urlBuilder.Append($"&code_challenge={Uri.EscapeDataString(codeChallenge)}");
        urlBuilder.Append("&code_challenge_method=S256");
        urlBuilder.Append($"&state={Uri.EscapeDataString(state)}");

        if (!string.IsNullOrEmpty(identityProviderHint) && !string.IsNullOrEmpty(_idpHintParam))
        {
            urlBuilder.Append($"&{Uri.EscapeDataString(_idpHintParam)}={Uri.EscapeDataString(identityProviderHint)}");
        }

        return new Uri(urlBuilder.ToString());
    }

    private Uri BuildRegistrationUrl(string codeChallenge, string state)
    {
        var urlBuilder = new StringBuilder(_registrationEndpoint);
        urlBuilder.Append($"?client_id={Uri.EscapeDataString(_oidcClientId)}");
        urlBuilder.Append($"&redirect_uri={Uri.EscapeDataString(_callbackUrl)}");
        urlBuilder.Append("&response_type=code");
        urlBuilder.Append("&scope=openid%20profile%20email");
        urlBuilder.Append($"&code_challenge={Uri.EscapeDataString(codeChallenge)}");
        urlBuilder.Append("&code_challenge_method=S256");
        urlBuilder.Append($"&state={Uri.EscapeDataString(state)}");

        return new Uri(urlBuilder.ToString());
    }
}
