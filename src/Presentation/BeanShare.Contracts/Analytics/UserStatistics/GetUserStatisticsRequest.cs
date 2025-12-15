namespace BeanShare.Contracts.Analytics.UserStatistics;

public sealed record GetUserStatisticsRequest
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
