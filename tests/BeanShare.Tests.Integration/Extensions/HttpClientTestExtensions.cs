using System.Net.Http.Json;

namespace BeanShare.Tests.Integration.Extensions;

public static class HttpClientTestExtensions
{
    public static HttpClient WithTestUser(this HttpClient client, Guid userId, string? email = null)
    {
        client.DefaultRequestHeaders.Remove("X-Test-UserId");
        client.DefaultRequestHeaders.Remove("X-Test-Email");

        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        if (email != null)
        {
            client.DefaultRequestHeaders.Add("X-Test-Email", email);
        }

        return client;
    }

    public static HttpClient AsDefaultUser(this HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("X-Test-UserId");
        client.DefaultRequestHeaders.Remove("X-Test-Email");
        return client;
    }

    public static async Task<TResponse?> PostAsJsonWithUserAsync<TRequest, TResponse>(
        this HttpClient client,
        string url,
        TRequest content,
        Guid userId,
        string? email = null,
        CancellationToken cancellationToken = default)
    {
        client.WithTestUser(userId, email);
        var response = await client.PostAsJsonAsync(url, content, cancellationToken);
        client.AsDefaultUser();

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }
}