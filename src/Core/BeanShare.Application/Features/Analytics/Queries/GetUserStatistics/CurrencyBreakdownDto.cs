using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Analytics.Queries.GetUserStatistics;
public sealed record CurrencyBreakdownDto
{
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal OriginalAmount { get; init; }
    public Money? ConvertedAmount { get; init; }
    public bool WasConverted { get; init; }
}
