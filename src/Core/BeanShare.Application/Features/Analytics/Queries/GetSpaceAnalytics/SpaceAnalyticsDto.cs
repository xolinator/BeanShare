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

public sealed record TopConsumerDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public int CupCount { get; init; }
    public Money? TotalCost { get; init; }
}

public sealed record PopularCoffeeDto
{
    public string CoffeeName { get; init; } = string.Empty;
    public int ConsumptionCount { get; init; }
    public decimal TotalGrams { get; init; }
    public double Percentage { get; init; }
}
