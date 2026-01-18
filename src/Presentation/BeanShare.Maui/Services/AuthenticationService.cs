using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;

namespace BeanShare.Maui.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationService> _logger;
    private UserInfo? _currentUser;

    private const string KeycloakAuthority = "http://localhost:8080/realms/beanshare";
    private const string KeycloakClientId = "beanshare-mobile";
    private const string CallbackScheme = "beanshare";
    private const string CallbackUrl = "beanshare://callback";

    private static readonly string AuthorizationEndpoint = $"{KeycloakAuthority}/protocol/openid-connect/auth";
    private static readonly string TokenEndpoint = $"{KeycloakAuthority}/protocol/openid-connect/token";
    private static readonly string EndSessionEndpoint = $"{KeycloakAuthority}/protocol/openid-connect/logout";

    public AuthenticationService(HttpClient httpClient, ILogger<AuthenticationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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

    public async Task<AuthResult> LoginWithKeycloakAsync(string? identityProviderHint = null)
    {
        try
        {
            _logger.LogInformation("Starting Keycloak PKCE authentication flow. IDP hint: {IdpHint}", identityProviderHint ?? "none");

            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            var state = GenerateRandomString(32);

            var authUrlBuilder = new StringBuilder(AuthorizationEndpoint);
            authUrlBuilder.Append($"?client_id={Uri.EscapeDataString(KeycloakClientId)}");
            authUrlBuilder.Append($"&redirect_uri={Uri.EscapeDataString(CallbackUrl)}");
            authUrlBuilder.Append("&response_type=code");
            authUrlBuilder.Append("&scope=openid%20profile%20email");
            authUrlBuilder.Append($"&code_challenge={Uri.EscapeDataString(codeChallenge)}");
            authUrlBuilder.Append("&code_challenge_method=S256");
            authUrlBuilder.Append($"&state={Uri.EscapeDataString(state)}");

            if (!string.IsNullOrEmpty(identityProviderHint))
            {
                authUrlBuilder.Append($"&kc_idp_hint={Uri.EscapeDataString(identityProviderHint)}");
            }

            var authUrl = new Uri(authUrlBuilder.ToString());
            var callbackUri = new Uri(CallbackUrl);

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

            return await ExchangeCodeForTokensAsync(code, codeVerifier);
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

    private async Task<AuthResult> ExchangeCodeForTokensAsync(string code, string codeVerifier)
    {
        try
        {
            _logger.LogInformation("Exchanging authorization code for tokens");

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = KeycloakClientId,
                ["code"] = code,
                ["redirect_uri"] = CallbackUrl,
                ["code_verifier"] = codeVerifier
            };

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token exchange failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return new AuthResult(false, null, "Failed to exchange authorization code for tokens.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
            if (tokenResponse == null)
            {
                _logger.LogError("Failed to parse token response");
                return new AuthResult(false, null, "Failed to parse token response.");
            }

            _logger.LogInformation("Token exchange successful. Access token received.");

            await StoreTokensAsync(tokenResponse);

            var userInfo = await ExtractUserInfoFromTokenAsync(tokenResponse.IdToken ?? tokenResponse.AccessToken);
            if (userInfo == null)
            {
                _logger.LogError("Failed to extract user info from token");
                return new AuthResult(false, null, "Failed to extract user information.");
            }

            _currentUser = userInfo;

            await SyncUserWithApiAsync(tokenResponse.AccessToken);

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");

            return new AuthResult(true, userInfo, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token exchange failed");
            return new AuthResult(false, null, $"Token exchange failed: {ex.Message}");
        }
    }

    private async Task SyncUserWithApiAsync(string accessToken)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = _httpClient.BaseAddress;
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await httpClient.PostAsync("/api/users/sync", null);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("User synced with API successfully");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to sync user with API: {StatusCode} - {Error}", response.StatusCode, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync user with API (non-fatal)");
        }
    }

    public async Task<bool> TryRefreshTokensAsync()
    {
        try
        {
            string? refreshToken = null;
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                refreshToken = await SecureStorage.Default.GetAsync("refresh_token");
            });

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogDebug("No refresh token available");
                return false;
            }

            _logger.LogInformation("Attempting to refresh tokens");

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = KeycloakClientId,
                ["refresh_token"] = refreshToken
            };

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token refresh failed: {StatusCode}", response.StatusCode);
                return false;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
            if (tokenResponse == null)
            {
                return false;
            }

            await StoreTokensAsync(tokenResponse);

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");

            _logger.LogInformation("Tokens refreshed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return false;
        }
    }

    private async Task StoreTokensAsync(KeycloakTokenResponse tokenResponse)
    {
        var tcs = new TaskCompletionSource<bool>();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await SecureStorage.Default.SetAsync("access_token", tokenResponse.AccessToken);
                await SecureStorage.Default.SetAsync("auth_token", tokenResponse.AccessToken);

                if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                {
                    await SecureStorage.Default.SetAsync("refresh_token", tokenResponse.RefreshToken);
                }

                if (!string.IsNullOrEmpty(tokenResponse.IdToken))
                {
                    await SecureStorage.Default.SetAsync("id_token", tokenResponse.IdToken);
                }

                var expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                await SecureStorage.Default.SetAsync("token_expires_at", expiresAt.ToString("O"));

                _logger.LogInformation("Tokens stored securely. Expires at: {ExpiresAt}", expiresAt);
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store tokens");
                tcs.SetException(ex);
            }
        });

        await tcs.Task;
    }

    private async Task<UserInfo?> ExtractUserInfoFromTokenAsync(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var sub = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value
                    ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
            var picture = jwtToken.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
            {
                _logger.LogError("Failed to parse user ID from token. Sub claim: {Sub}", sub);
                return null;
            }

            var tcs = new TaskCompletionSource<bool>();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await SecureStorage.Default.SetAsync("user_id", userId.ToString());
                    await SecureStorage.Default.SetAsync("user_email", email ?? "");
                    await SecureStorage.Default.SetAsync("user_name", name ?? "");
                    if (!string.IsNullOrEmpty(picture))
                    {
                        await SecureStorage.Default.SetAsync("user_picture", picture);
                    }
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            await tcs.Task;

            UserContextStub.SetUser(userId, email ?? "");

            return new UserInfo(userId, email ?? "", name ?? "", picture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract user info from token");
            return null;
        }
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
                var logoutUrl = $"{EndSessionEndpoint}?id_token_hint={Uri.EscapeDataString(idToken)}&post_logout_redirect_uri={Uri.EscapeDataString(CallbackUrl)}";
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
                var refreshed = await TryRefreshTokensAsync();
                return refreshed;
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

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(bytes);
    }

    private static string GenerateRandomString(int length)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_expires_in")]
        public int RefreshExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}
