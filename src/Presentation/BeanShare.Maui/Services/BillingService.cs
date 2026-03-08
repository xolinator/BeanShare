using System.Net.Http.Json;
using BeanShare.Contracts.Billing;

namespace BeanShare.Maui.Services;

public class BillingService : IBillingService
{
    private readonly HttpClient _httpClient;

    public BillingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<BillingPeriodSummaryDto>> GetSpaceBillingPeriodsAsync(Guid spaceId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<BillingPeriodSummaryDto>>($"/api/spaces/{spaceId}/billing-periods");
            return result ?? new List<BillingPeriodSummaryDto>();
        }
        catch
        {
            return new List<BillingPeriodSummaryDto>();
        }
    }

    public async Task<BillingPeriodDto> GetBillingPeriodByIdAsync(Guid billingPeriodId)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<BillingPeriodDto>($"/api/billing-periods/{billingPeriodId}");
            return result!;
        }
        catch
        {
            return null!;
        }
    }

    public async Task<BillingPeriodDto> CreateBillingPeriodAsync(Guid spaceId, CreateBillingPeriodDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/spaces/{spaceId}/billing-periods", dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<BillingPeriodDto>();
            return result!;
        }
        catch
        {
            return null!;
        }
    }

    public async Task OpenBillingPeriodAsync(Guid billingPeriodId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"/api/billing-periods/{billingPeriodId}/open", null);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Silently fail for now
        }
    }

    public async Task CloseBillingPeriodAsync(Guid billingPeriodId)
    {
        try
        {
            var response = await _httpClient.PutAsync($"/api/billing-periods/{billingPeriodId}/close", null);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Silently fail for now
        }
    }
}
