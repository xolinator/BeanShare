using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Analytics.Queries.GetUserStatistics;
public sealed record UserStatisticsDto
{
    public int TotalCups { get; init; }
    public int CupsThisMonth { get; init; }
    public int CupsThisWeek { get; init; }
    public int CupsToday { get; init; }
    public double AverageCupsPerDay { get; init; }
    public Money? TotalCost { get; init; }
    public bool IsCostFullyConverted { get; init; }
    public IReadOnlyList<CurrencyBreakdownDto> CostBreakdown { get; init; } = [];
    public string? PreferredCurrencyCode { get; init; }
    public string? MostConsumedCoffeeType { get; init; }
    public DateTime? FirstConsumptionDate { get; init; }
    public DateTime? LastConsumptionDate { get; init; }
    public Dictionary<string, int> CoffeeTypeBreakdown { get; init; } = new();
    public Dictionary<DateTime, int> DailyConsumptionTrend { get; init; } = new();
    public int ActiveSpacesCount { get; init; }
}
