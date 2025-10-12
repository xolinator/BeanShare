using System.Net.Http.Json;
using BeanShare.Application.Features.CoffeeStock.Dtos;

namespace BeanShare.Maui.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;

    public StockService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CoffeeStockDto?> GetSpaceStockAsync(Guid spaceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<CoffeeStockDto>($"/api/spaces/{spaceId}/stock");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> AddStockPurchaseAsync(Guid spaceId, string productName, string coffeeType, int quantityGrams, decimal totalCost, string currency)
    {
        try
        {
            var request = new
            {
                ProductName = productName,
                ProductBrand = "Generic",
                ProductType = coffeeType,
                QuantityGrams = quantityGrams,
                CostAmount = totalCost,
                CostCurrency = currency,
                Vendor = "Store",
                PurchasedAt = DateTime.UtcNow
            };

            var response = await _httpClient.PostAsJsonAsync($"/api/spaces/{spaceId}/stock/purchases", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
