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
        return await RecordConsumptionAsync(spaceId, "Coffee", "Generic", "Ground", grams, consumedAt);
    }

    public async Task<bool> RecordConsumptionAsync(Guid spaceId, string productName, string productBrand, string productType, int grams, DateTime consumedAt, string? presetName = null, Guid? forUserId = null)
    {
        try
        {
            var request = new
            {
                SpaceId = spaceId,
                ProductName = productName,
                ProductBrand = productBrand,
                ProductType = productType,
                QuantityGrams = (decimal)grams,
                ConsumedAt = consumedAt,
                PresetName = presetName,
                ForUserId = forUserId
            };

            var response = await _httpClient.PostAsJsonAsync("/api/consumptions", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ImportConsumptionCsvResult?> ImportCsvAsync(Guid spaceId, IReadOnlyList<ImportConsumptionCsvRow> rows)
    {
        try
        {
            var request = new
            {
                SpaceId = spaceId,
                Rows = rows.Select(r => new
                {
                    r.RowNumber,
                    r.Email,
                    r.ProductName,
                    r.ProductBrand,
                    r.ProductType,
                    r.QuantityGrams,
                    r.ConsumedAt
                }).ToList()
            };

            var response = await _httpClient.PostAsJsonAsync("/api/consumptions/import-csv", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ImportConsumptionCsvResult>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateConsumptionAsync(Guid id, Guid spaceId, string productName, string productBrand, string productType, decimal quantityGrams, DateTime consumedAt)
    {
        try
        {
            var request = new
            {
                Id = id,
                SpaceId = spaceId,
                ProductName = productName,
                ProductBrand = productBrand,
                ProductType = productType,
                QuantityGrams = quantityGrams,
                ConsumedAt = consumedAt
            };

            var response = await _httpClient.PutAsJsonAsync($"/api/consumptions/{id}", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteConsumptionAsync(Guid id, Guid spaceId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/consumptions/{id}?spaceId={spaceId}");
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
        int pageSize = 20,
        Guid? memberUserId = null)
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

            if (memberUserId.HasValue)
                queryParams["memberUserId"] = memberUserId.Value.ToString();

            var url = $"/api/me/consumption/history?{queryParams}";
            return await _httpClient.GetFromJsonAsync<ConsumptionHistoryDto>(url);
        }
        catch
        {
            return null;
        }
    }
}
