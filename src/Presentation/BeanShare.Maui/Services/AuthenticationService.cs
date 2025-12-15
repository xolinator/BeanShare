using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BeanShare.Maui.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationService> _logger;
    private UserInfo? _currentUser;

    private const string GoogleClientId = "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com";
    private const string FacebookAppId = "YOUR_FACEBOOK_APP_ID";
    private const string CallbackScheme = "beanshare";

    public AuthenticationService(HttpClient httpClient, ILogger<AuthenticationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {Email}", email);

            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new
            {
                Email = email,
                Password = password
            });

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    _logger.LogInformation("Login successful for user: {Email}", email);
                    return await HandleAuthSuccessAsync(authResponse);
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Login failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new AuthResult(false, null, "Invalid email or password");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API connection failed during login");
            return new AuthResult(false, null, "Unable to connect to the server. Please check your connection and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return new AuthResult(false, null, "An error occurred during login");
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string name, string password)
    {
        try
        {
            _logger.LogInformation("Attempting registration for user: {Email}", email);

            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", new
            {
                Email = email,
                Name = name,
                Password = password
            });

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    _logger.LogInformation("Registration successful for user: {Email}", email);
                    return await HandleAuthSuccessAsync(authResponse);
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Registration failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new AuthResult(false, null, "Registration failed. Email may already be in use.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "API connection failed during registration");
            return new AuthResult(false, null, "Unable to connect to the server. Please check your connection and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return new AuthResult(false, null, "An error occurred during registration");
        }
    }

    public async Task<AuthResult> LoginWithGoogleAsync()
    {
        try
        {
            _logger.LogInformation("Starting Google OAuth flow");

            var authUrl = new Uri($"https://accounts.google.com/o/oauth2/v2/auth?" +
                $"client_id={GoogleClientId}&" +
                $"redirect_uri={CallbackScheme}://callback&" +
                $"response_type=code&" +
                $"scope=openid%20email%20profile");

            var callbackUrl = new Uri($"{CallbackScheme}://callback");

            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = authUrl,
                    CallbackUrl = callbackUrl,
                    PrefersEphemeralWebBrowserSession = true
                });

            var code = result?.Properties.GetValueOrDefault("code");
            var idToken = result?.Properties.GetValueOrDefault("id_token");
            var accessToken = result?.AccessToken;

            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(idToken) && string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Google OAuth returned no tokens");
                return new AuthResult(false, null, "Google authentication failed. No tokens received.");
            }

            return await ExchangeTokenAsync("Google", idToken ?? accessToken ?? code!);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Google login was cancelled by user");
            return new AuthResult(false, null, "Google authentication was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google login failed");
            return new AuthResult(false, null, $"Google authentication failed: {ex.Message}");
        }
    }

    public async Task<AuthResult> LoginWithFacebookAsync()
    {
        try
        {
            _logger.LogInformation("Starting Facebook OAuth flow");

            var authUrl = new Uri($"https://www.facebook.com/v18.0/dialog/oauth?" +
                $"client_id={FacebookAppId}&" +
                $"redirect_uri={CallbackScheme}://callback&" +
                $"response_type=token&" +
                $"scope=email,public_profile");

            var callbackUrl = new Uri($"{CallbackScheme}://callback");

            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = authUrl,
                    CallbackUrl = callbackUrl,
                    PrefersEphemeralWebBrowserSession = true
                });

            var accessToken = result?.AccessToken;

            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = result?.Properties.GetValueOrDefault("access_token");
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Facebook OAuth returned no access token");
                return new AuthResult(false, null, "Facebook authentication failed. No token received.");
            }

            return await ExchangeTokenAsync("Facebook", accessToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Facebook login was cancelled by user");
            return new AuthResult(false, null, "Facebook authentication was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facebook login failed");
            return new AuthResult(false, null, $"Facebook authentication failed: {ex.Message}");
        }
    }

    private async Task<AuthResult> ExchangeTokenAsync(string provider, string token)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/external", new
            {
                Provider = provider,
                IdToken = token,
                AccessToken = token
            });

            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    return await HandleAuthSuccessAsync(authResponse);
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Token exchange failed: {Error}", errorContent);
            return new AuthResult(false, null, $"{provider} authentication failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token exchange failed for {Provider}", provider);
            return new AuthResult(false, null, $"{provider} authentication failed");
        }
    }

    private async Task<AuthResult> HandleAuthSuccessAsync(AuthResponse authResponse)
    {
        var user = new UserInfo(
            authResponse.UserId,
            authResponse.Email,
            authResponse.Name,
            authResponse.PictureUrl);

        _currentUser = user;

        await StoreAuthDataAsync(
            authResponse.UserId,
            authResponse.Email,
            authResponse.Name,
            authResponse.Token,
            authResponse.PictureUrl);

        UserContextStub.SetUser(authResponse.UserId, authResponse.Email);

        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authResponse.Token}");

        _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
        _httpClient.DefaultRequestHeaders.Add("X-User-Id", authResponse.UserId.ToString());

        return new AuthResult(true, user, null);
    }

    private async Task StoreAuthDataAsync(Guid userId, string email, string name, string token, string? pictureUrl = null)
    {
        var tcs = new TaskCompletionSource<bool>();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await SecureStorage.Default.SetAsync("user_id", userId.ToString());
                await SecureStorage.Default.SetAsync("user_email", email);
                await SecureStorage.Default.SetAsync("user_name", name);
                await SecureStorage.Default.SetAsync("auth_token", token);
                if (!string.IsNullOrEmpty(pictureUrl))
                {
                    await SecureStorage.Default.SetAsync("user_picture", pictureUrl);
                }
                _logger.LogInformation("Auth data stored for user {UserId}", userId);
                tcs.SetResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store auth data");
                tcs.SetException(ex);
            }
        });

        await tcs.Task;
    }

    public async Task LogoutAsync()
    {
        _logger.LogInformation("Starting logout process");
        _currentUser = null;
        UserContextStub.ClearUser();

        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Remove("X-User-Id");

        var tcs = new TaskCompletionSource<bool>();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                SecureStorage.Default.Remove("user_id");
                SecureStorage.Default.Remove("user_email");
                SecureStorage.Default.Remove("user_name");
                SecureStorage.Default.Remove("auth_token");
                SecureStorage.Default.Remove("user_picture");
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
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        string? userId = null;
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            userId = await SecureStorage.Default.GetAsync("user_id");
        });
        return !string.IsNullOrEmpty(userId);
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        string? userId = null;
        string? email = null;
        string? name = null;
        string? pictureUrl = null;
        string? token = null;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            userId = await SecureStorage.Default.GetAsync("user_id");
            email = await SecureStorage.Default.GetAsync("user_email");
            name = await SecureStorage.Default.GetAsync("user_name");
            pictureUrl = await SecureStorage.Default.GetAsync("user_picture");
            token = await SecureStorage.Default.GetAsync("auth_token");
        });

        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id))
        {
            _currentUser = new UserInfo(id, email ?? "", name ?? "", pictureUrl);
            UserContextStub.SetUser(id, email ?? "");

            if (!string.IsNullOrEmpty(token) && !token.StartsWith("mock_"))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            }

            _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
            _httpClient.DefaultRequestHeaders.Add("X-User-Id", id.ToString());

            return _currentUser;
        }

        return null;
    }

    private record AuthResponse(
        string Token,
        Guid UserId,
        string Email,
        string Name,
        string? PictureUrl,
        string Provider,
        DateTime ExpiresAt);
}
