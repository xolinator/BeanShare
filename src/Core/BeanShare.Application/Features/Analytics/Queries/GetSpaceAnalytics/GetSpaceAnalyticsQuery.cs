using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Analytics.Queries.GetSpaceAnalytics;
public sealed record GetSpaceAnalyticsQuery : IRequest<SpaceAnalyticsDto>
{
    public SpaceId SpaceId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

    public GetSpaceAnalyticsQuery(SpaceId spaceId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        SpaceId = spaceId;
        FromDate = fromDate;
        ToDate = toDate;
    }
}
