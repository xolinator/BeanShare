using BeanShare.Application.Abstractions;
using BeanShare.Domain.Common;
using System.Diagnostics;

namespace BeanShare.Maui.Services;

/// <summary>
/// Stub implementation of IUserContext for MAUI client.
/// In MAUI, user context is stored in SecureStorage.
/// </summary>
public class UserContextStub : IUserContext
{
    public UserId CurrentUserId => GetUserIdFromStorage();
    public string Email => GetUserEmailFromStorage();
    public IReadOnlyCollection<string> Roles => new List<string> { "User" };

    private UserId GetUserIdFromStorage()
    {
        try
        {
            var userIdTask = MainThread.InvokeOnMainThreadAsync(async () =>
                await SecureStorage.Default.GetAsync("user_id"));

            var userId = userIdTask.GetAwaiter().GetResult();

            if (string.IsNullOrEmpty(userId))
                return UserId.New();

            if (Guid.TryParse(userId, out var guid))
            {
                return new UserId(guid);
            }

            return UserId.New();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserContextStub] Error getting user ID: {ex.Message}");
            return UserId.New();
        }
    }

    private string GetUserEmailFromStorage()
    {
        try
        {
            var emailTask = MainThread.InvokeOnMainThreadAsync(async () =>
                await SecureStorage.Default.GetAsync("user_email"));

            return emailTask.GetAwaiter().GetResult() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserContextStub] Error getting user email: {ex.Message}");
            return string.Empty;
        }
    }
}