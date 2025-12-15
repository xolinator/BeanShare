namespace BeanShare.Contracts.Analytics.UserStatistics;

public sealed record GetUserStatisticsResponse
{
    public required int TotalCups { get; init; }
    public required int CupsThisMonth { get; init; }
    public required int CupsThisWeek { get; init; }
    public required int CupsToday { get; init; }
    public required double AverageCupsPerDay { get; init; }
    public decimal? TotalCost { get; init; }
    public string? TotalCostCurrency { get; init; }
    public required bool IsCostFullyConverted { get; init; }
    public required List<CurrencyBreakdownResponse> CostBreakdown { get; init; }
    public string? PreferredCurrencyCode { get; init; }
    public string? MostConsumedCoffeeType { get; init; }
    public DateTime? FirstConsumptionDate { get; init; }
    public DateTime? LastConsumptionDate { get; init; }
    public required Dictionary<string, int> CoffeeTypeBreakdown { get; init; }
    public required Dictionary<DateTime, int> DailyConsumptionTrend { get; init; }
    public required int ActiveSpacesCount { get; init; }
}
