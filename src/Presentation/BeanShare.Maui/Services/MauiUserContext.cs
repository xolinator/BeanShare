using System.Diagnostics;

namespace BeanShare.Maui.Services;

public interface IMauiUserContext
{
    Task<Guid?> GetCurrentUserIdAsync();
    Task<string?> GetCurrentUserNameAsync();
    Task<string?> GetCurrentUserEmailAsync();
    Task<Guid?> GetSelectedSpaceIdAsync();
    Task SetSelectedSpaceIdAsync(Guid spaceId);
    Task ClearUserContextAsync();
}

public class MauiUserContext : IMauiUserContext
{
    public async Task<Guid?> GetCurrentUserIdAsync()
    {
        try
        {
            var userId = await MainThread.InvokeOnMainThreadAsync(async () =>
                await SecureStorage.Default.GetAsync("user_id"));

            if (string.IsNullOrEmpty(userId))
                return null;

            return Guid.TryParse(userId, out var guid) ? guid : null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MauiUserContext] Error getting user ID: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetCurrentUserNameAsync()
    {
        try
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
                await SecureStorage.Default.GetAsync("user_name"));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MauiUserContext] Error getting user name: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetCurrentUserEmailAsync()
    {
        try
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
                await SecureStorage.Default.GetAsync("user_email"));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MauiUserContext] Error getting user email: {ex.Message}");
            return null;
        }
    }

    public async Task<Guid?> GetSelectedSpaceIdAsync()
    {
        try
        {
            var spaceId = await MainThread.InvokeOnMainThreadAsync(async () =>
                await SecureStorage.Default.GetAsync("selected_space_id"));

            if (string.IsNullOrEmpty(spaceId))
                return null;

            return Guid.TryParse(spaceId, out var guid) ? guid : null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MauiUserContext] Error getting selected space ID: {ex.Message}");
            return null;
        }
    }

    public async Task SetSelectedSpaceIdAsync(Guid spaceId)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await SecureStorage.Default.SetAsync("selected_space_id", spaceId.ToString()));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MauiUserContext] Error setting selected space ID: {ex.Message}");
        }
    }

    public async Task ClearUserContextAsync()
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SecureStorage.Default.RemoveAll();
                return Task.CompletedTask;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MauiUserContext] Error clearing user context: {ex.Message}");
        }
    }
}