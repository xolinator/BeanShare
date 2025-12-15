using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using System.Diagnostics;

namespace BeanShare.Maui.Services;

/// <summary>
/// Stub implementation of IUserContext for MAUI client.
/// Uses cached values to avoid deadlocks with SecureStorage on the UI thread.
/// </summary>
public class UserContextStub : IUserContext
{
    // Static cache to avoid deadlocks - set during login/app startup
    private static UserId? _cachedUserId;
    private static string? _cachedEmail;
    private static bool _initialized;

    public UserId CurrentUserId => _cachedUserId ?? GetUserIdFallback();
    public string Email => _cachedEmail ?? string.Empty;
    public IReadOnlyCollection<string> Roles => new List<string> { "User" };

    /// <summary>
    /// Initialize the cache from SecureStorage. Must be called on app startup or after login.
    /// </summary>
    public static async Task InitializeAsync()
    {
        if (_initialized) return;

        try
        {
            string? userId = null;
            string? email = null;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                userId = await SecureStorage.Default.GetAsync("user_id");
                email = await SecureStorage.Default.GetAsync("user_email");
            });

            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var guid))
            {
                _cachedUserId = new UserId(guid);
            }
            _cachedEmail = email ?? string.Empty;
            _initialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserContextStub] Error initializing: {ex.Message}");
        }
    }

    /// <summary>
    /// Set the cached user info. Called during login.
    /// </summary>
    public static void SetUser(Guid userId, string email)
    {
        _cachedUserId = new UserId(userId);
        _cachedEmail = email;
        _initialized = true;
    }

    /// <summary>
    /// Clear the cached user info. Called during logout.
    /// </summary>
    public static void ClearUser()
    {
        _cachedUserId = null;
        _cachedEmail = null;
        _initialized = false;
    }

    private UserId GetUserIdFallback()
    {
        // Return a default UserId if not initialized - pages should handle null-like scenarios
        Debug.WriteLine("[UserContextStub] Warning: User ID not cached, returning new UserId");
        return UserId.New();
    }
}