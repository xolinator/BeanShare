using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.CoffeeStock;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Extensions;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class CoffeeStockEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;
    private readonly HttpClient _client;

    public CoffeeStockEndpointsTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
        _client.AsDefaultUser();
    }

    [Fact]
    public async Task AddStockPurchase_WithValidRequest_ShouldReturnCreatedPurchase()
    {
        var spaceId = await CreateTestSpace();
        var request = new AddStockPurchaseRequest
        {
            BodySpaceId = spaceId,
            ProductName = "Premium Espresso Blend",
            ProductBrand = "Blue Mountain Coffee",
            ProductType = "Espresso",
            QuantityGrams = 1000,
            CostAmount = 25.99m,
            CostCurrency = "USD",
            Vendor = "Local Coffee Roasters",
            PurchasedAt = DateTime.UtcNow.AddDays(-1)
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/purchases", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var purchase = await response.Content.ReadFromJsonAsync<StockPurchaseResponse>();
        purchase.Should().NotBeNull();
        purchase!.Id.Should().NotBeEmpty();
        purchase.ProductName.Should().Be("Premium Espresso Blend");
        purchase.ProductBrand.Should().Be("Blue Mountain Coffee");
        purchase.ProductType.Should().Be("Espresso");
        purchase.QuantityGrams.Should().Be(1000);
        purchase.CostAmount.Should().Be(25.99m);
        purchase.CostCurrency.Should().Be("USD");
        purchase.Vendor.Should().Be("Local Coffee Roasters");
        purchase.Message.Should().Contain("Successfully added");
        purchase.CostPerGram.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task AddStockPurchase_WithMismatchedSpaceId_ShouldReturnBadRequest()
    {
        var spaceId = await CreateTestSpace();
        var differentSpaceId = Guid.NewGuid();
        var request = new AddStockPurchaseRequest
        {
            BodySpaceId = differentSpaceId,
            ProductName = "Test Product",
            ProductBrand = "Test Brand",
            ProductType = "Espresso",
            QuantityGrams = 500,
            CostAmount = 15.00m,
            CostCurrency = "USD",
            Vendor = "Test Vendor",
            PurchasedAt = DateTime.UtcNow.AddDays(-1)
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/purchases", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddStockPurchase_WithInvalidData_ShouldReturnBadRequest()
    {
        var spaceId = await CreateTestSpace();
        var request = new AddStockPurchaseRequest
        {
            BodySpaceId = spaceId,
            ProductName = "",
            ProductBrand = "Test Brand",
            ProductType = "Espresso",
            QuantityGrams = 0,
            CostAmount = -5.00m,
            CostCurrency = "USD",
            Vendor = "Test Vendor",
            PurchasedAt = DateTime.UtcNow.AddDays(-1)
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/purchases", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddStockPurchase_WithNonexistentSpace_ShouldReturnError()
    {
        var nonexistentSpaceId = Guid.NewGuid();
        var request = new AddStockPurchaseRequest
        {
            BodySpaceId = nonexistentSpaceId,
            ProductName = "Test Product",
            ProductBrand = "Test Brand",
            ProductType = "Espresso",
            QuantityGrams = 500,
            CostAmount = 15.00m,
            CostCurrency = "USD",
            Vendor = "Test Vendor",
            PurchasedAt = DateTime.UtcNow.AddDays(-1)
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{nonexistentSpaceId}/stock/purchases", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSpaceStock_WithExistingStock_ShouldReturnStockData()
    {
        var spaceId = await CreateTestSpace();
        await AddTestPurchase(spaceId, "Premium Blend", "Blue Mountain", 1000, 25.99m);
        await AddTestPurchase(spaceId, "House Roast", "Local Roasters", 500, 15.50m);

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/stock");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stock = await response.Content.ReadFromJsonAsync<SpaceStockResponse>();
        stock.Should().NotBeNull();
        stock!.SpaceId.Should().Be(spaceId);
        stock.PurchaseCount.Should().Be(2);
        stock.ProductVarietyCount.Should().Be(2);
        stock.TotalCurrentStockGrams.Should().Be(1500);
        stock.TotalInvestmentAmount.Should().Be(41.49m);
        stock.TotalInvestmentCurrency.Should().Be("USD");
        stock.StockLevels.Should().HaveCount(2);
        stock.RecentPurchases.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSpaceStock_WithNoStock_ShouldReturnEmptyStock()
    {
        var spaceId = await CreateTestSpace();

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/stock");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stock = await response.Content.ReadFromJsonAsync<SpaceStockResponse>();
        stock.Should().NotBeNull();
        stock!.SpaceId.Should().Be(spaceId);
        stock.PurchaseCount.Should().Be(0);
        stock.ProductVarietyCount.Should().Be(0);
        stock.TotalCurrentStockGrams.Should().Be(0);
        stock.StockLevels.Should().BeEmpty();
        stock.RecentPurchases.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSpaceStock_WithNonexistentSpace_ShouldReturnError()
    {
        var nonexistentSpaceId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/spaces/{nonexistentSpaceId}/stock");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLowStockAlerts_WithDefaultThreshold_ShouldReturnAlerts()
    {
        var spaceId = await CreateTestSpace();
        await AddTestPurchase(spaceId, "Premium Blend", "Blue Mountain", 1000, 25.99m);
        await AddTestPurchase(spaceId, "House Roast", "Local Roasters", 500, 15.50m);

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/stock/alerts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var alerts = await response.Content.ReadFromJsonAsync<LowStockAlertsResponse>();
        alerts.Should().NotBeNull();
        alerts!.SpaceId.Should().Be(spaceId);
        alerts.AlertCount.Should().BeGreaterOrEqualTo(0);
        alerts.Alerts.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLowStockAlerts_WithCustomThreshold_ShouldReturnAlerts()
    {
        var spaceId = await CreateTestSpace();
        await AddTestPurchase(spaceId, "Premium Blend", "Blue Mountain", 1000, 25.99m);

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/stock/alerts?threshold=1500");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var alerts = await response.Content.ReadFromJsonAsync<LowStockAlertsResponse>();
        alerts.Should().NotBeNull();
        alerts!.SpaceId.Should().Be(spaceId);
        alerts.Alerts.Should().NotBeNull();
        alerts.AlertCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetLowStockAlerts_WithNonexistentSpace_ShouldReturnError()
    {
        var nonexistentSpaceId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/spaces/{nonexistentSpaceId}/stock/alerts");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StockWorkflow_EndToEnd_ShouldWork()
    {
        var spaceId = await CreateTestSpace();

        var purchase1 = await AddTestPurchase(spaceId, "Premium Espresso", "Blue Mountain", 1000, 30.00m);
        var purchase2 = await AddTestPurchase(spaceId, "House Blend", "Local Roasters", 500, 15.00m);

        // Verify stock reflects both purchases
        var stockResponse = await _client.GetAsync($"/api/spaces/{spaceId}/stock");
        stockResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var stock = await stockResponse.Content.ReadFromJsonAsync<SpaceStockResponse>();
        stock.Should().NotBeNull();
        stock!.PurchaseCount.Should().Be(2);
        stock.ProductVarietyCount.Should().Be(2);
        stock.TotalCurrentStockGrams.Should().Be(1500);
        stock.TotalInvestmentAmount.Should().Be(45.00m);

        // Verify both products are in stock levels
        var premiumProduct = stock.StockLevels.FirstOrDefault(sl => sl.ProductName == "Premium Espresso");
        premiumProduct.Should().NotBeNull();
        premiumProduct!.CurrentStockGrams.Should().Be(1000);

        var houseProduct = stock.StockLevels.FirstOrDefault(sl => sl.ProductName == "House Blend");
        houseProduct.Should().NotBeNull();
        houseProduct!.CurrentStockGrams.Should().Be(500);

        // Verify recent purchases include both
        stock.RecentPurchases.Should().HaveCount(2);
        stock.RecentPurchases.Should().Contain(p => p.ProductName == "Premium Espresso");
        stock.RecentPurchases.Should().Contain(p => p.ProductName == "House Blend");

        // Test low stock alerts with high threshold
        var alertsResponse = await _client.GetAsync($"/api/spaces/{spaceId}/stock/alerts?threshold=2000");
        alertsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var alerts = await alertsResponse.Content.ReadFromJsonAsync<LowStockAlertsResponse>();
        alerts.Should().NotBeNull();
        alerts!.AlertCount.Should().BeGreaterThan(0); // Both products should be below 2000g threshold
    }

    [Fact]
    public async Task DeleteStockPurchase_ShouldRemovePurchaseAndReduceStockLevel()
    {
        var spaceId = await CreateTestSpace();
        var purchase = await AddTestPurchase(spaceId, "Premium Espresso", "Blue Mountain", 1000, 30.00m);

        // Verify initial state
        var stockBefore = await (await _client.GetAsync($"/api/spaces/{spaceId}/stock"))
            .Content.ReadFromJsonAsync<SpaceStockResponse>();
        stockBefore!.PurchaseCount.Should().Be(1);
        stockBefore.TotalCurrentStockGrams.Should().Be(1000);
        stockBefore.StockLevels.Single(sl => sl.ProductName == "Premium Espresso").CurrentStockGrams.Should().Be(1000);

        // Delete the purchase
        var deleteResponse = await _client.DeleteAsync($"/api/spaces/{spaceId}/stock/purchases/{purchase.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify stock is updated after deletion
        var stockAfter = await (await _client.GetAsync($"/api/spaces/{spaceId}/stock"))
            .Content.ReadFromJsonAsync<SpaceStockResponse>();
        stockAfter.Should().NotBeNull();
        stockAfter!.PurchaseCount.Should().Be(0);
        stockAfter.TotalCurrentStockGrams.Should().Be(0);
        var stockLevel = stockAfter.StockLevels.FirstOrDefault(sl => sl.ProductName == "Premium Espresso");
        if (stockLevel != null)
            stockLevel.CurrentStockGrams.Should().Be(0);
    }

    [Fact]
    public async Task DeleteStockPurchase_WithMultiplePurchasesSameProduct_ShouldOnlyReduceByDeletedAmount()
    {
        var spaceId = await CreateTestSpace();
        var purchase1 = await AddTestPurchase(spaceId, "Premium Espresso", "Blue Mountain", 1000, 30.00m);
        _ = await AddTestPurchase(spaceId, "Premium Espresso", "Blue Mountain", 500, 15.00m);

        // Delete only the first purchase
        var deleteResponse = await _client.DeleteAsync($"/api/spaces/{spaceId}/stock/purchases/{purchase1.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Remaining stock should reflect only the second purchase
        var stockAfter = await (await _client.GetAsync($"/api/spaces/{spaceId}/stock"))
            .Content.ReadFromJsonAsync<SpaceStockResponse>();
        stockAfter.Should().NotBeNull();
        stockAfter!.PurchaseCount.Should().Be(1);
        stockAfter.TotalCurrentStockGrams.Should().Be(500);
        stockAfter.StockLevels.Single(sl => sl.ProductName == "Premium Espresso").CurrentStockGrams.Should().Be(500);
    }

    private async Task<Guid> CreateTestSpace()
    {
        var createRequest = new CreateSpaceRequest { Name = $"Test Space {Guid.NewGuid()}" };
        var response = await _client.PostAsJsonAsync("/api/spaces", createRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdSpace = await response.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        return createdSpace!.SpaceId;
    }

    private async Task<StockPurchaseResponse> AddTestPurchase(
        Guid spaceId,
        string productName,
        string productBrand,
        decimal quantityGrams,
        decimal costAmount,
        string productType = "Espresso",
        string vendor = "Test Vendor",
        string currency = "USD")
    {
        var request = new AddStockPurchaseRequest
        {
            BodySpaceId = spaceId,
            ProductName = productName,
            ProductBrand = productBrand,
            ProductType = productType,
            QuantityGrams = quantityGrams,
            CostAmount = costAmount,
            CostCurrency = currency,
            Vendor = vendor,
            PurchasedAt = DateTime.UtcNow.AddDays(-1)
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/purchases", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var purchase = await response.Content.ReadFromJsonAsync<StockPurchaseResponse>();
        return purchase!;
    }
}