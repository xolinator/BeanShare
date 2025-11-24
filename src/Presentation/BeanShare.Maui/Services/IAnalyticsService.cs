using BeanShare.Application.Features.Analytics.Queries.GetSpaceAnalytics;
using BeanShare.Application.Features.Analytics.Queries.GetUserStatistics;

namespace BeanShare.Maui.Services;

public interface IAnalyticsService
{
    Task<SpaceAnalyticsDto?> GetSpaceAnalyticsAsync(Guid spaceId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<UserStatisticsDto?> GetUserStatisticsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<UserStatisticsDto?> GetMyStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}