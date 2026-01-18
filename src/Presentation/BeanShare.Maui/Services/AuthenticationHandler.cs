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
        string? accessToken = null;

        if (MainThread.IsMainThread)
        {
            accessToken = await SecureStorage.Default.GetAsync("access_token");
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                accessToken = await SecureStorage.Default.GetAsync("access_token");
            });
        }

        System.Diagnostics.Debug.WriteLine($"[AuthenticationHandler] Request URL: {request.RequestUri}");
        System.Diagnostics.Debug.WriteLine($"[AuthenticationHandler] Access token present: {!string.IsNullOrEmpty(accessToken)}");

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            System.Diagnostics.Debug.WriteLine("[AuthenticationHandler] Added Bearer token to request");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[AuthenticationHandler] WARNING: No access token found in SecureStorage!");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
