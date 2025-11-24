using System.Net.Http.Json;
using BeanShare.Application.Features.Analytics.Queries.GetSpaceAnalytics;
using BeanShare.Application.Features.Analytics.Queries.GetUserStatistics;
using BeanShare.Contracts.Analytics;

namespace BeanShare.Maui.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly HttpClient _httpClient;

    public AnalyticsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SpaceAnalyticsDto?> GetSpaceAnalyticsAsync(Guid spaceId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var url = $"/api/spaces/{spaceId}/analytics";
            if (fromDate.HasValue || toDate.HasValue)
            {
                var queryParams = new List<string>();
                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                url += "?" + string.Join("&", queryParams);
            }

            var response = await _httpClient.GetFromJsonAsync<GetSpaceAnalyticsResponse>(url);
            if (response == null) return null;

            return new SpaceAnalyticsDto
            {
                TotalMembers = response.TotalMembers,
                TotalConsumptionsThisMonth = response.TotalConsumptionsThisMonth,
                TotalConsumptionsAllTime = response.TotalConsumptionsAllTime,
                TopConsumers = response.TopConsumers.Select(tc => new Application.Features.Analytics.Queries.GetSpaceAnalytics.TopConsumerDto
                {
                    UserId = tc.UserId,
                    UserName = tc.UserName,
                    CupCount = tc.CupCount,
                    TotalCost = tc.TotalCost.HasValue
                        ? Domain.ValueObjects.Money.Create(tc.TotalCost.Value, Domain.ValueObjects.Currency.Create("USD"))
                        : null
                }).ToList(),
                PopularCoffeeTypes = response.PopularCoffeeTypes.Select(pc => new Application.Features.Analytics.Queries.GetSpaceAnalytics.PopularCoffeeDto
                {
                    CoffeeName = pc.CoffeeName,
                    ConsumptionCount = pc.ConsumptionCount,
                    TotalGrams = pc.TotalGrams,
                    Percentage = pc.Percentage
                }).ToList(),
                CurrentStockValue = response.CurrentStockValue.HasValue
                    ? Domain.ValueObjects.Money.Create(response.CurrentStockValue.Value, Domain.ValueObjects.Currency.Create("USD"))
                    : null,
                CurrentStockGrams = response.CurrentStockGrams,
                DailyConsumptionTrend = response.DailyConsumptionTrend,
                HourlyConsumptionPattern = response.HourlyConsumptionPattern,
                TotalCostThisMonth = response.TotalCostThisMonth.HasValue
                    ? Domain.ValueObjects.Money.Create(response.TotalCostThisMonth.Value, Domain.ValueObjects.Currency.Create("USD"))
                    : null
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserStatisticsDto?> GetUserStatisticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var url = $"/api/users/{userId}/statistics";
            if (fromDate.HasValue || toDate.HasValue)
            {
                var queryParams = new List<string>();
                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                url += "?" + string.Join("&", queryParams);
            }

            var response = await _httpClient.GetFromJsonAsync<GetUserStatisticsResponse>(url);
            if (response == null) return null;

            return ConvertToUserStatisticsDto(response);
        }
        catch
        {
            return null;
        }
    }

    public async Task<UserStatisticsDto?> GetMyStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var url = "/api/me/statistics";
            if (fromDate.HasValue || toDate.HasValue)
            {
                var queryParams = new List<string>();
                if (fromDate.HasValue)
                    queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");
                if (toDate.HasValue)
                    queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");
                url += "?" + string.Join("&", queryParams);
            }

            var response = await _httpClient.GetFromJsonAsync<GetUserStatisticsResponse>(url);
            if (response == null) return null;

            return ConvertToUserStatisticsDto(response);
        }
        catch
        {
            return null;
        }
    }

    private UserStatisticsDto ConvertToUserStatisticsDto(GetUserStatisticsResponse response)
    {
        return new UserStatisticsDto
        {
            TotalCups = response.TotalCups,
            CupsThisMonth = response.CupsThisMonth,
            CupsThisWeek = response.CupsThisWeek,
            CupsToday = response.CupsToday,
            AverageCupsPerDay = response.AverageCupsPerDay,
            TotalCost = response.TotalCost.HasValue
                ? Domain.ValueObjects.Money.Create(response.TotalCost.Value, Domain.ValueObjects.Currency.Create("USD"))
                : null,
            MostConsumedCoffeeType = response.MostConsumedCoffeeType,
            FirstConsumptionDate = response.FirstConsumptionDate,
            LastConsumptionDate = response.LastConsumptionDate,
            CoffeeTypeBreakdown = response.CoffeeTypeBreakdown,
            DailyConsumptionTrend = response.DailyConsumptionTrend,
            ActiveSpacesCount = response.ActiveSpacesCount
        };
    }
}