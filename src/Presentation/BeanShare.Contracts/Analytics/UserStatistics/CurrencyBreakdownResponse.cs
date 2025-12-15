namespace BeanShare.Contracts.Analytics.UserStatistics;

public sealed record CurrencyBreakdownResponse
{
    public required string CurrencyCode { get; init; }
    public required decimal OriginalAmount { get; init; }
    public decimal? ConvertedAmount { get; init; }
    public string? ConvertedCurrency { get; init; }
    public required bool WasConverted { get; init; }
}
