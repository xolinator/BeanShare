using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.CoffeeStock;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class StockAlertEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly HttpClient _client;

    public StockAlertEndpointsTests(PostgreSqlFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    private async Task<Guid> CreateSpaceAsync(string name = "Stock Alert Test Space")
    {
        var request = new CreateSpaceRequest { Name = name };
        var response = await _client.PostAsJsonAsync("/api/spaces", request);
        var created = await response.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        return created!.SpaceId;
    }

    private async Task AddStockAsync(Guid spaceId, string name = "Test Coffee", decimal grams = 500)
    {
        var request = new AddStockPurchaseRequest
        {
            ProductName = name,
            ProductBrand = "TestBrand",
            ProductType = "Espresso",
            QuantityGrams = grams,
            CostAmount = 25.00m,
            CostCurrency = "USD",
            Vendor = "Test Vendor",
            PurchasedAt = DateTime.UtcNow
        };
        await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/purchases", request);
    }

    [Fact]
    public async Task GetLowStockAlerts_WithValidSpace_ShouldReturnOk()
    {
        var spaceId = await CreateSpaceAsync();
        await AddStockAsync(spaceId);

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/stock/alerts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLowStockAlerts_WithInvalidSpace_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync($"/api/spaces/{Guid.NewGuid()}/stock/alerts");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConsumeStock_WithValidRequest_ShouldReturnOk()
    {
        var spaceId = await CreateSpaceAsync("Stock Consume Test");
        await AddStockAsync(spaceId);

        var request = new ConsumeStockRequest
        {
            SpaceId = spaceId,
            ProductName = "Test Coffee",
            ProductBrand = "TestBrand",
            ProductType = "Espresso",
            QuantityGrams = 14
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/consume", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
    }

    [Fact]
    public async Task ConsumeStock_ExceedingAvailable_ShouldReturnBadRequest()
    {
        var spaceId = await CreateSpaceAsync("Stock Overconsume Test");
        await AddStockAsync(spaceId, grams: 10);

        var request = new ConsumeStockRequest
        {
            SpaceId = spaceId,
            ProductName = "Test Coffee",
            ProductBrand = "TestBrand",
            ProductType = "Espresso",
            QuantityGrams = 999
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/consume", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StockWorkflow_AddConsumeCheckAlerts_ShouldWork()
    {
        var spaceId = await CreateSpaceAsync("Stock Workflow Test");
        await AddStockAsync(spaceId, grams: 100);

        // Get stock
        var stockResponse = await _client.GetAsync($"/api/spaces/{spaceId}/stock");
        stockResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Consume most of it
        var consumeRequest = new ConsumeStockRequest
        {
            SpaceId = spaceId,
            ProductName = "Test Coffee",
            ProductBrand = "TestBrand",
            ProductType = "Espresso",
            QuantityGrams = 90
        };
        var consumeResponse = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/consume", consumeRequest);
        consumeResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        // Check low stock alerts
        var alertsResponse = await _client.GetAsync($"/api/spaces/{spaceId}/stock/alerts");
        alertsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
