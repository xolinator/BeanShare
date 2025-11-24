namespace BeanShare.Contracts.Analytics;

// Space Analytics
public sealed record GetSpaceAnalyticsRequest
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed record GetSpaceAnalyticsResponse
{
    public required int TotalMembers { get; init; }
    public required int TotalConsumptionsThisMonth { get; init; }
    public required int TotalConsumptionsAllTime { get; init; }
    public required List<TopConsumerDto> TopConsumers { get; init; }
    public required List<PopularCoffeeDto> PopularCoffeeTypes { get; init; }
    public decimal? CurrentStockValue { get; init; }
    public required decimal CurrentStockGrams { get; init; }
    public required Dictionary<DateTime, int> DailyConsumptionTrend { get; init; }
    public required Dictionary<int, int> HourlyConsumptionPattern { get; init; }
    public decimal? TotalCostThisMonth { get; init; }
}

public sealed record TopConsumerDto
{
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required int CupCount { get; init; }
    public decimal? TotalCost { get; init; }
}

public sealed record PopularCoffeeDto
{
    public required string CoffeeName { get; init; }
    public required int ConsumptionCount { get; init; }
    public required decimal TotalGrams { get; init; }
    public required double Percentage { get; init; }
}

// User Statistics
public sealed record GetUserStatisticsRequest
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public sealed record GetUserStatisticsResponse
{
    public required int TotalCups { get; init; }
    public required int CupsThisMonth { get; init; }
    public required int CupsThisWeek { get; init; }
    public required int CupsToday { get; init; }
    public required double AverageCupsPerDay { get; init; }
    public decimal? TotalCost { get; init; }
    public string? MostConsumedCoffeeType { get; init; }
    public DateTime? FirstConsumptionDate { get; init; }
    public DateTime? LastConsumptionDate { get; init; }
    public required Dictionary<string, int> CoffeeTypeBreakdown { get; init; }
    public required Dictionary<DateTime, int> DailyConsumptionTrend { get; init; }
    public required int ActiveSpacesCount { get; init; }
}