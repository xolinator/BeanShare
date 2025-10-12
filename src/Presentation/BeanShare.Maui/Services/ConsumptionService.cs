using System.Net.Http.Json;
using BeanShare.Application.Features.Consumption.Queries;

namespace BeanShare.Maui.Services;

public class ConsumptionService : IConsumptionService
{
    private readonly HttpClient _httpClient;

    public ConsumptionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetRecentConsumptionsResult?> GetRecentConsumptionsAsync(Guid spaceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<GetRecentConsumptionsResult>($"/api/spaces/{spaceId}/consumption");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> RecordConsumptionAsync(Guid spaceId, int grams, DateTime consumedAt)
    {
        try
        {
            var request = new
            {
                ProductName = "Coffee",
                ProductBrand = "Generic",
                ProductType = "Ground",
                QuantityGrams = grams,
                ConsumedAt = consumedAt
            };

            var response = await _httpClient.PostAsJsonAsync($"/api/spaces/{spaceId}/consumption", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
