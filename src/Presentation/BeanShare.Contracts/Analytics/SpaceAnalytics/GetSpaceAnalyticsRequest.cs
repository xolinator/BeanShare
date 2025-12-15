namespace BeanShare.Contracts.Analytics.SpaceAnalytics;

public sealed record GetSpaceAnalyticsRequest
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
