using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.ActiveQrCodes;
using BeanShare.Contracts.CoffeeStock;
using BeanShare.Contracts.SemiAuthQr;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class SemiAuthQrEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly HttpClient _client;

    public SemiAuthQrEndpointsTests(PostgreSqlFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task RecordSemiAuthQrConsumption_WithValidDeviceToken_ShouldReturnCreated()
    {
        var spaceId = await CreateSpaceAsync("SemiAuth QR Happy Path");
        await AddStockAsync(spaceId);
        var qrId = await CreateQrAsync(spaceId);
        var token = await IssueTokenAsync();

        var request = new RecordSemiAuthQrConsumptionRequest
        {
            DeviceId = token.DeviceId,
            DeviceToken = token.Token,
            QuantityGrams = 12
        };

        var response = await _client.PostAsJsonAsync($"/api/qr-codes/{qrId}/semi-auth-consumptions", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task RecordSemiAuthQrConsumption_WithoutToken_ShouldReturnUnauthorized()
    {
        var qrId = Guid.NewGuid();
        var request = new RecordSemiAuthQrConsumptionRequest
        {
            DeviceId = Guid.NewGuid().ToString(),
            DeviceToken = string.Empty
        };

        var response = await _client.PostAsJsonAsync($"/api/qr-codes/{qrId}/semi-auth-consumptions", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecordSemiAuthQrConsumption_WithMismatchedDevice_ShouldReturnUnauthorized()
    {
        var spaceId = await CreateSpaceAsync("SemiAuth QR Replay Guard");
        await AddStockAsync(spaceId);
        var qrId = await CreateQrAsync(spaceId);
        var token = await IssueTokenAsync();

        var request = new RecordSemiAuthQrConsumptionRequest
        {
            DeviceId = Guid.NewGuid().ToString(),
            DeviceToken = token.Token,
            QuantityGrams = 10
        };

        var response = await _client.PostAsJsonAsync($"/api/qr-codes/{qrId}/semi-auth-consumptions", request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<Guid> CreateSpaceAsync(string name)
    {
        var request = new CreateSpaceRequest { Name = name };
        var response = await _client.PostAsJsonAsync("/api/spaces", request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        return payload!.SpaceId;
    }

    private async Task AddStockAsync(Guid spaceId)
    {
        var request = new AddStockPurchaseRequest
        {
            ProductName = "SemiAuth Beans",
            ProductBrand = "BeanShare",
            ProductType = "Espresso",
            QuantityGrams = 500,
            CostAmount = 25.0m,
            CostCurrency = "USD",
            Vendor = "Vendor",
            PurchasedAt = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/purchases", request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreateQrAsync(Guid spaceId)
    {
        var request = new CreateActiveQrCodeRequest
        {
            SpaceId = spaceId,
            Label = "Kitchen",
            ProductName = "SemiAuth Beans",
            ProductBrand = "BeanShare",
            ProductType = "Espresso",
            RecipeName = "Espresso",
            DefaultGrams = 10
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{spaceId}/qr-codes", request);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ActiveQrCodeResponse>();
        return payload!.Id;
    }

    private async Task<DeviceLogTokenResponse> IssueTokenAsync()
    {
        var request = new IssueDeviceLogTokenRequest
        {
            DeviceId = Guid.NewGuid().ToString()
        };

        var response = await _client.PostAsJsonAsync("/api/me/semi-auth-qr/device-token", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<DeviceLogTokenResponse>())!;
    }
}
