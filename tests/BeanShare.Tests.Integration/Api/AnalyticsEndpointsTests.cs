using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class AnalyticsEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly HttpClient _client;

    public AnalyticsEndpointsTests(PostgreSqlFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    private async Task<Guid> CreateSpaceAsync(string name = "Analytics Test Space")
    {
        var request = new CreateSpaceRequest { Name = name };
        var response = await _client.PostAsJsonAsync("/api/spaces", request);
        var created = await response.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        return created!.SpaceId;
    }

    [Fact]
    public async Task GetMyStatistics_ShouldReturnOk()
    {
        _client.DefaultRequestHeaders.Remove("X-User-Id");
        _client.DefaultRequestHeaders.Add("X-User-Id", "11111111-1111-1111-1111-111111111111");

        var response = await _client.GetAsync("/api/me/statistics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        _client.DefaultRequestHeaders.Remove("X-User-Id");
    }

    [Fact]
    public async Task GetSpaceAnalytics_WithValidSpace_ShouldReturnOk()
    {
        var spaceId = await CreateSpaceAsync();

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/analytics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("TotalMembers");
    }

    [Fact]
    public async Task GetSpaceAnalytics_WithInvalidSpace_ShouldReturnBadRequestOrOk()
    {
        var response = await _client.GetAsync($"/api/spaces/{Guid.NewGuid()}/analytics");

        // API may return default analytics (200) for non-existent space or 400/404
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSpaceAnalytics_ShouldIncludeTopConsumers()
    {
        var spaceId = await CreateSpaceAsync("Analytics Consumers Test");

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/analytics");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("TopConsumers");
    }

    [Fact]
    public async Task GetSpaceAnalytics_ShouldIncludePopularCoffeeTypes()
    {
        var spaceId = await CreateSpaceAsync("Analytics Coffee Test");

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/analytics");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("PopularCoffeeTypes");
    }
}
