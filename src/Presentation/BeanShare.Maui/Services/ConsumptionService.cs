using System.Net.Http.Json;
using System.Web;
using BeanShare.Application.Features.Consumption.Queries;
using BeanShare.Application.Features.Consumption.Dtos;

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

    public async Task<ConsumptionHistoryDto?> GetUserHistoryAsync(
        Guid? spaceId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? billingPeriodId = null,
        int pageNumber = 1,
        int pageSize = 20)
    {
        try
        {
            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["pageNumber"] = pageNumber.ToString();
            queryParams["pageSize"] = pageSize.ToString();

            if (spaceId.HasValue)
                queryParams["spaceId"] = spaceId.Value.ToString();

            if (startDate.HasValue)
                queryParams["startDate"] = startDate.Value.ToString("yyyy-MM-dd");

            if (endDate.HasValue)
                queryParams["endDate"] = endDate.Value.ToString("yyyy-MM-dd");

            if (billingPeriodId.HasValue)
                queryParams["billingPeriodId"] = billingPeriodId.Value.ToString();

            var url = $"/api/me/consumption/history?{queryParams}";
            return await _httpClient.GetFromJsonAsync<ConsumptionHistoryDto>(url);
        }
        catch
        {
            return null;
        }
    }
}
