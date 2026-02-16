using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Extensions;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class BillingEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly HttpClient _client;

    public BillingEndpointsTests(PostgreSqlFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    private async Task<Guid> CreateSpaceAsync(string name = "Billing Test Space")
    {
        var request = new CreateSpaceRequest { Name = name };
        var response = await _client.PostAsJsonAsync("/api/spaces", request);
        var created = await response.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        return created!.SpaceId;
    }

    [Fact]
    public async Task GetSpaceBillingPeriods_WithValidSpace_ShouldReturnOk()
    {
        var spaceId = await CreateSpaceAsync();

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/billing-periods");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSpaceBillingPeriods_WithInvalidSpace_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync($"/api/spaces/{Guid.NewGuid()}/billing-periods");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBillingPeriod_WithValidRequest_ShouldReturnCreated()
    {
        var spaceId = await CreateSpaceAsync("Billing Create Test");

        var request = new
        {
            SpaceId = spaceId,
            Name = "January 2026",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(30)
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/billing-periods", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBillingPeriodById_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/billing-periods/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BillingPeriodWorkflow_CreateAndList_ShouldWork()
    {
        var spaceId = await CreateSpaceAsync("Billing Workflow Test");

        var createRequest = new
        {
            SpaceId = spaceId,
            Name = "Test Period",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(30)
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/billing-periods", createRequest);
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

        var listResponse = await _client.GetAsync($"/api/spaces/{spaceId}/billing-periods");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await listResponse.Content.ReadAsStringAsync();
        content.Should().Contain("Test Period");
    }
}
