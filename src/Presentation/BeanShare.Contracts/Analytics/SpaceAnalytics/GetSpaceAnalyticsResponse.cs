namespace BeanShare.Contracts.Analytics.SpaceAnalytics;

public sealed record GetSpaceAnalyticsResponse
{
    public required int TotalMembers { get; init; }
    public required int TotalConsumptionsThisMonth { get; init; }
    public required int TotalConsumptionsAllTime { get; init; }
    public required List<TopConsumerDto> TopConsumers { get; init; }
    public required List<PopularCoffeeDto> PopularCoffeeTypes { get; init; }
    public decimal? CurrentStockValue { get; init; }
    public string? CurrentStockValueCurrency { get; init; }
    public required decimal CurrentStockGrams { get; init; }
    public required Dictionary<DateTime, int> DailyConsumptionTrend { get; init; }
    public required Dictionary<int, int> HourlyConsumptionPattern { get; init; }
    public decimal? TotalCostThisMonth { get; init; }
    public string? TotalCostThisMonthCurrency { get; init; }
}
