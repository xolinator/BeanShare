namespace BeanShare.Contracts.Analytics.SpaceAnalytics;

public sealed record TopConsumerDto
{
    public required Guid UserId { get; init; }
    public required string UserName { get; init; }
    public required int CupCount { get; init; }
    public decimal? TotalCost { get; init; }
    public string? TotalCostCurrency { get; init; }
}
