using System.Net.Http.Json;
using BeanShare.Contracts.Settlement;

namespace BeanShare.Maui.Services;

public class SettlementService : ISettlementService
{
    private readonly HttpClient _httpClient;

    public SettlementService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SettlementSummaryDto>> GetSpaceSettlementsAsync(Guid spaceId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<SettlementSummaryDto>>($"/api/spaces/{spaceId}/settlements");
            return result ?? new List<SettlementSummaryDto>();
        }
        catch
        {
            return new List<SettlementSummaryDto>();
        }
    }

    public async Task<SettlementDto> GetSettlementByIdAsync(Guid settlementId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<SettlementDto>($"/api/settlements/{settlementId}");
            return result!;
        }
        catch
        {
            return null!;
        }
    }

    public async Task<SettlementDto> GenerateSettlementAsync(Guid billingPeriodId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/billing-periods/{billingPeriodId}/settlement", null);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<SettlementDto>();
            return result!;
        }
        catch
        {
            return null!;
        }
    }
}
