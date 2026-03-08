using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BeanShare.Maui.Services;

public class AuthenticationHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    public AuthenticationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await AttachTokenAsync(request);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthenticationHandler] 401 received for {request.RequestUri} - attempting token refresh");

            var refreshed = await TryRefreshAsync();
            if (refreshed)
            {
                var retryRequest = await CloneRequestAsync(request);
                await AttachTokenAsync(retryRequest);
                response.Dispose();
                response = await base.SendAsync(retryRequest, cancellationToken);
                System.Diagnostics.Debug.WriteLine($"[AuthenticationHandler] Retry after refresh: {(int)response.StatusCode} for {request.RequestUri}");
            }
        }

        return response;
    }

    private static async Task AttachTokenAsync(HttpRequestMessage request)
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

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    private async Task<bool> TryRefreshAsync()
    {
        if (!await _refreshLock.WaitAsync(TimeSpan.FromSeconds(10)))
            return false;

        try
        {
            // Check if another thread already refreshed
            string? expiresAtStr = null;
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                expiresAtStr = await SecureStorage.Default.GetAsync("token_expires_at");
            });

            if (!string.IsNullOrEmpty(expiresAtStr) && DateTime.TryParse(expiresAtStr, out var expiresAt))
            {
                if (expiresAt > DateTime.UtcNow.AddSeconds(30))
                {
                    // Token was already refreshed by another concurrent request
                    return true;
                }
            }

            var authService = _serviceProvider.GetRequiredService<IAuthenticationService>();
            return await authService.RefreshTokenAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthenticationHandler] Refresh failed: {ex.Message}");
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);
        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);
            if (request.Content.Headers.ContentType != null)
                clone.Content.Headers.ContentType = request.Content.Headers.ContentType;
        }

        foreach (var header in request.Headers)
        {
            if (header.Key != "Authorization")
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
