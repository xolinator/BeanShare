using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Analytics.Queries.GetSpaceAnalytics;
public sealed record SpaceAnalyticsDto
{
    public int TotalMembers { get; init; }
    public int TotalConsumptionsThisMonth { get; init; }
    public int TotalConsumptionsAllTime { get; init; }
    public List<TopConsumerDto> TopConsumers { get; init; } = new();
    public List<PopularCoffeeDto> PopularCoffeeTypes { get; init; } = new();
    public Money? CurrentStockValue { get; init; }
    public decimal CurrentStockGrams { get; init; }
    public Dictionary<DateTime, int> DailyConsumptionTrend { get; init; } = new();
    public Dictionary<int, int> HourlyConsumptionPattern { get; init; } = new();
    public Money? TotalCostThisMonth { get; init; }
}

