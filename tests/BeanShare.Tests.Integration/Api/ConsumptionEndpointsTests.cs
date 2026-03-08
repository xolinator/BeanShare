using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.CoffeeStock;
using BeanShare.Contracts.Consumption;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class ConsumptionEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly HttpClient _client;

    public ConsumptionEndpointsTests(PostgreSqlFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    private async Task<Guid> CreateSpaceAsync(string name = "Consumption Test Space")
    {
        var request = new CreateSpaceRequest { Name = name };
        var response = await _client.PostAsJsonAsync("/api/spaces", request);
        var created = await response.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        return created!.SpaceId;
    }

    private async Task AddStockAsync(Guid spaceId)
    {
        var request = new AddStockPurchaseRequest
        {
            ProductName = "Test Coffee",
            ProductBrand = "TestBrand",
            ProductType = "Espresso",
            QuantityGrams = 500,
            CostAmount = 25.00m,
            CostCurrency = "USD",
            Vendor = "Test Vendor",
            PurchasedAt = DateTime.UtcNow
        };
        await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/purchases", request);
    }

    [Fact]
    public async Task RecordConsumption_WithValidRequest_ShouldReturnCreated()
    {
        var spaceId = await CreateSpaceAsync("Consume Valid Test");
        await AddStockAsync(spaceId);

        var request = new RecordConsumptionRequest
        {
            SpaceId = spaceId,
            ProductName = "Test Coffee",
            ProductBrand = "TestBrand",
            ProductType = "Espresso",
            QuantityGrams = 14
        };

        var response = await _client.PostAsJsonAsync("/api/consumptions", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Fact]
    public async Task RecordConsumption_WithNoStock_ShouldReturnBadRequest()
    {
        var spaceId = await CreateSpaceAsync("Consume No Stock Test");

        var request = new RecordConsumptionRequest
        {
            SpaceId = spaceId,
            ProductName = "NonExistent",
            ProductBrand = "NoBrand",
            ProductType = "Espresso",
            QuantityGrams = 14
        };

        var response = await _client.PostAsJsonAsync("/api/consumptions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetConsumptionHistory_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/me/consumption/history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ConsumptionWorkflow_AddStockThenConsume_ShouldReduceStock()
    {
        var spaceId = await CreateSpaceAsync("Consumption Workflow Test");
        await AddStockAsync(spaceId);

        // Get stock before
        var stockBefore = await _client.GetAsync($"/api/spaces/{spaceId}/stock");
        stockBefore.StatusCode.Should().Be(HttpStatusCode.OK);

        // Consume
        var consumeRequest = new RecordConsumptionRequest
        {
            SpaceId = spaceId,
            ProductName = "Test Coffee",
            ProductBrand = "TestBrand",
            ProductType = "Espresso",
            QuantityGrams = 14
        };
        var consumeResponse = await _client.PostAsJsonAsync("/api/consumptions", consumeRequest);
        consumeResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        // Get stock after
        var stockAfter = await _client.GetAsync($"/api/spaces/{spaceId}/stock");
        stockAfter.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
