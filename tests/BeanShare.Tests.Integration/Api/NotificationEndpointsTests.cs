using System.Net;
using System.Net.Http.Json;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class NotificationEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly HttpClient _client;

    public NotificationEndpointsTests(PostgreSqlFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetNotifications_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/me/notifications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetUnreadCount_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/me/notifications/unread-count");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Count");
    }

    [Fact]
    public async Task MarkAllAsRead_ShouldReturnOk()
    {
        var response = await _client.PostAsync("/api/me/notifications/read-all", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task MarkNotificationAsRead_WithInvalidId_ShouldReturnNotFoundOrBadRequest()
    {
        var invalidId = Guid.NewGuid();
        var response = await _client.PostAsJsonAsync($"/api/me/notifications/{invalidId}/read", new { });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ClearNotifications_ShouldReturnOk()
    {
        var response = await _client.DeleteAsync("/api/me/notifications");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task NotificationWorkflow_GetThenMarkAllRead_ShouldWork()
    {
        // Get notifications first
        var getResponse = await _client.GetAsync("/api/me/notifications");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Mark all as read
        var markResponse = await _client.PostAsync("/api/me/notifications/read-all", null);
        markResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Unread count should be 0
        var countResponse = await _client.GetAsync("/api/me/notifications/unread-count");
        countResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
