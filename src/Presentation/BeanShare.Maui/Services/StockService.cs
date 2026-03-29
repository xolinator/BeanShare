using System.Net.Http.Json;
using BeanShare.Application.Features.CoffeeStock.Dtos;

namespace BeanShare.Maui.Services;

public class StockService : IStockService
{
    private readonly HttpClient _httpClient;
    private readonly ApiResponseCache _cache;

    public StockService(HttpClient httpClient, ApiResponseCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<CoffeeStockDto?> GetSpaceStockAsync(Guid spaceId)
    {
        var cacheKey = $"stock:{spaceId}";
        var cached = _cache.Get<CoffeeStockDto>(cacheKey);
        if (cached != null)
            return cached;

        var stale = _cache.GetStale<CoffeeStockDto>(cacheKey);

        try
        {
            var result = await _httpClient.GetFromJsonAsync<CoffeeStockDto>($"/api/spaces/{spaceId}/stock");
            if (result != null)
                _cache.Set(cacheKey, result);
            return result;
        }
        catch
        {
            return stale;
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
            if (response.IsSuccessStatusCode)
            {
                _cache.Invalidate("stock:");
            }
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
