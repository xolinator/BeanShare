using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.CoffeeStock;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace BeanShare.Tests.Integration.Api;

public sealed class CoffeeStockDiagnosticTest : IClassFixture<PostgreSqlFixture>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public CoffeeStockDiagnosticTest(PostgreSqlFixture fixture, ITestOutputHelper output)
    {
        _client = fixture.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task DiagnoseAddStockPurchaseFailure()
    {
        var createSpaceRequest = new CreateSpaceRequest { Name = "Diagnostic Test Space" };
        var createSpaceResponse = await _client.PostAsJsonAsync("/api/spaces", createSpaceRequest);

        _output.WriteLine($"Create Space Status: {createSpaceResponse.StatusCode}");
        var createSpaceBody = await createSpaceResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"Create Space Response: {createSpaceBody}");

        var createdSpace = await createSpaceResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        var spaceId = createdSpace!.SpaceId;
        _output.WriteLine($"Created Space ID: {spaceId}");

        var addStockRequest = new AddStockPurchaseRequest
        {
            BodySpaceId = spaceId,
            ProductName = "Test Coffee",
            ProductBrand = "Test Brand",
            ProductType = "Espresso",
            QuantityGrams = 1000,
            CostAmount = 25.99m,
            CostCurrency = "USD",
            Vendor = "Test Vendor",
            PurchasedAt = DateTime.UtcNow.AddDays(-1)
        };

        var addStockResponse = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/purchases", addStockRequest);

        _output.WriteLine($"Add Stock Status: {addStockResponse.StatusCode}");
        var addStockBody = await addStockResponse.Content.ReadAsStringAsync();
        _output.WriteLine($"Add Stock Response Body:");
        _output.WriteLine(addStockBody);

        Assert.Equal(HttpStatusCode.Created, addStockResponse.StatusCode);
    }
}
