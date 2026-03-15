using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace BeanShare.Maui.Services;

/// <summary>
/// Handles OAuth/OIDC token operations: exchange, refresh, storage, and user extraction.
/// </summary>
public class OidcTokenHandler
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly string _oidcClientId;
    private readonly string _callbackUrl;
    private readonly string _tokenEndpoint;

    public OidcTokenHandler(
        HttpClient httpClient,
        ILogger logger,
        string oidcClientId,
        string callbackUrl,
        string tokenEndpoint)
    {
        _httpClient = httpClient;
        _logger = logger;
        _oidcClientId = oidcClientId;
        _callbackUrl = callbackUrl;
        _tokenEndpoint = tokenEndpoint;
    }

    /// <summary>
    /// Exchanges an authorization code for access/refresh tokens.
    /// </summary>
    public async Task<AuthResult> ExchangeCodeForTokensAsync(string code, string codeVerifier)
    {
        try
        {
            _logger.LogInformation("Exchanging authorization code for tokens");

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = _oidcClientId,
                ["code"] = code,
                ["redirect_uri"] = _callbackUrl,
                ["code_verifier"] = codeVerifier
            };

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(_tokenEndpoint, new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token exchange failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return new AuthResult(false, null, "Failed to exchange authorization code for tokens.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<OidcTokenResponse>();
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

    /// <summary>
    /// Attempts to refresh tokens using the stored refresh token.
    /// </summary>
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
                ["client_id"] = _oidcClientId,
                ["refresh_token"] = refreshToken
            };

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(_tokenEndpoint, new FormUrlEncodedContent(tokenRequest));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token refresh failed: {StatusCode}", response.StatusCode);
                return false;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<OidcTokenResponse>();
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

    /// <summary>
    /// Extracts user information from a JWT token and stores it in secure storage.
    /// </summary>
    public async Task<UserInfo?> ExtractUserInfoFromTokenAsync(string token)
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

    private async Task StoreTokensAsync(OidcTokenResponse tokenResponse)
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

    internal class OidcTokenResponse
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
