using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.SemiAuthQr;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class SemiAuthQrEndpointAuthorizationTests : IClassFixture<JwtTestFixture>
{
    private readonly HttpClient _client;

    public SemiAuthQrEndpointAuthorizationTests(JwtTestFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task IssueDeviceToken_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        var request = new IssueDeviceLogTokenRequest
        {
            DeviceId = Guid.NewGuid().ToString()
        };

        var response = await _client.PostAsJsonAsync("/api/me/semi-auth-qr/device-token", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SemiAuthQrConsume_AnonymousEndpoint_ShouldNotRequireAuthenticationChallenge()
    {
        var request = new RecordSemiAuthQrConsumptionRequest
        {
            DeviceId = Guid.NewGuid().ToString(),
            DeviceToken = "missing"
        };

        var response = await _client.PostAsJsonAsync($"/api/qr-codes/{Guid.NewGuid()}/semi-auth-consumptions", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
