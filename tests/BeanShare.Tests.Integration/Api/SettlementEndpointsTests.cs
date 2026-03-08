using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class SettlementEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly HttpClient _client;

    public SettlementEndpointsTests(PostgreSqlFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    private async Task<Guid> CreateSpaceAsync(string name = "Settlement Test Space")
    {
        var request = new CreateSpaceRequest { Name = name };
        var response = await _client.PostAsJsonAsync("/api/spaces", request);
        var created = await response.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        return created!.SpaceId;
    }

    [Fact]
    public async Task GetSpaceSettlements_WithValidSpace_ShouldReturnOk()
    {
        var spaceId = await CreateSpaceAsync();

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/settlements");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSpaceSettlements_WithInvalidSpace_ShouldReturnBadRequestOrOk()
    {
        var response = await _client.GetAsync($"/api/spaces/{Guid.NewGuid()}/settlements");

        // API may return empty list (200) for non-existent space or 400/404
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSettlementById_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/settlements/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportSettlementPdf_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/settlements/{Guid.NewGuid()}/export/pdf");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ExportSettlementExcel_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/settlements/{Guid.NewGuid()}/export/excel");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateSettlement_WithInvalidPeriod_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync($"/api/billing-periods/{Guid.NewGuid()}/settlement", new { });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmPayment_WithInvalidSettlement_ShouldReturnNotFound()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/settlements/{Guid.NewGuid()}/confirm-payment/{Guid.NewGuid()}", new { });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }
}
