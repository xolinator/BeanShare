using System.Net.Http.Json;

namespace BeanShare.Maui.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private UserInfo? _currentUser;

    public AuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var seededUsers = new Dictionary<string, (string password, Guid id, string name)>
            {
                ["john.smith@beanshare.com"] = ("Password123!", Guid.Parse("11111111-1111-1111-1111-111111111111"), "John Smith"),
                ["sarah.johnson@beanshare.com"] = ("Password123!", Guid.Parse("22222222-2222-2222-2222-222222222222"), "Sarah Johnson"),
                ["mike.wilson@beanshare.com"] = ("Password123!", Guid.Parse("33333333-3333-3333-3333-333333333333"), "Mike Wilson"),
                ["emma.davis@beanshare.com"] = ("Password123!", Guid.Parse("44444444-4444-4444-4444-444444444444"), "Emma Davis"),
                ["alex.brown@beanshare.com"] = ("Password123!", Guid.Parse("55555555-5555-5555-5555-555555555555"), "Alex Brown"),
                ["lisa.martinez@beanshare.com"] = ("Password123!", Guid.Parse("66666666-6666-6666-6666-666666666666"), "Lisa Martinez"),
                ["david.garcia@beanshare.com"] = ("Password123!", Guid.Parse("77777777-7777-7777-7777-777777777777"), "David Garcia"),
                ["test@beanshare.com"] = ("test123", Guid.Parse("88888888-8888-8888-8888-888888888888"), "Test User")
            };

            var emailLower = email.ToLowerInvariant();
            if (seededUsers.ContainsKey(emailLower) && seededUsers[emailLower].password == password)
            {
                var userData = seededUsers[emailLower];
                var user = new UserInfo(userData.id, email, userData.name);

                _currentUser = user;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await SecureStorage.Default.SetAsync("user_id", userData.id.ToString());
                    await SecureStorage.Default.SetAsync("user_email", email);
                    await SecureStorage.Default.SetAsync("user_name", userData.name);
                    await SecureStorage.Default.SetAsync("auth_token", "mock_token_" + userData.id);
                });

                _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
                _httpClient.DefaultRequestHeaders.Add("X-User-Id", userData.id.ToString());

                return new AuthResult(true, user, null);
            }

            return new AuthResult(false, null, "Invalid email or password");
        }
        catch (Exception ex)
        {
            return new AuthResult(false, null, ex.Message);
        }
    }

    public async Task<AuthResult> RegisterAsync(string email, string name, string password)
    {
        try
        {

            var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var user = new UserInfo(userId, email, name);

            _currentUser = user;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await SecureStorage.Default.SetAsync("user_id", userId.ToString());
                await SecureStorage.Default.SetAsync("user_email", email);
                await SecureStorage.Default.SetAsync("user_name", name);
            });

            return new AuthResult(true, user, null);
        }
        catch (Exception ex)
        {
            return new AuthResult(false, null, ex.Message);
        }
    }

    public async Task LogoutAsync()
    {
        _currentUser = null;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            SecureStorage.Default.Remove("user_id");
            SecureStorage.Default.Remove("user_email");
            SecureStorage.Default.Remove("user_name");
        });
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

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            userId = await SecureStorage.Default.GetAsync("user_id");
            email = await SecureStorage.Default.GetAsync("user_email");
            name = await SecureStorage.Default.GetAsync("user_name");
        });

        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var id))
        {
            _currentUser = new UserInfo(id, email ?? "", name ?? "");
            return _currentUser;
        }

        return null;
    }
}
