using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BeanShare.Maui.Services;

public class AuthenticationHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? userId = null;
        string? authToken = null;

        if (MainThread.IsMainThread)
        {
            userId = await SecureStorage.Default.GetAsync("user_id");
            authToken = await SecureStorage.Default.GetAsync("auth_token");
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                userId = await SecureStorage.Default.GetAsync("user_id");
                authToken = await SecureStorage.Default.GetAsync("auth_token");
            });
        }

        System.Diagnostics.Debug.WriteLine($"[AuthenticationHandler] User ID from SecureStorage: {userId ?? "NULL"}");
        System.Diagnostics.Debug.WriteLine($"[AuthenticationHandler] Request URL: {request.RequestUri}");

        if (!string.IsNullOrEmpty(userId))
        {
            request.Headers.Remove("X-User-Id");
            request.Headers.Add("X-User-Id", userId);
            System.Diagnostics.Debug.WriteLine($"[AuthenticationHandler] Added X-User-Id header: {userId}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[AuthenticationHandler] WARNING: No user ID found in SecureStorage!");
        }

        if (!string.IsNullOrEmpty(authToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}