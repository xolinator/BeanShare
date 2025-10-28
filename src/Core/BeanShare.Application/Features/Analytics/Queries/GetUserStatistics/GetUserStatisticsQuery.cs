using BeanShare.Domain.Common;
using MediatR;

namespace BeanShare.Application.Features.Analytics.Queries.GetUserStatistics;

public record GetUserStatisticsQuery : IRequest<UserStatisticsDto>
{
    public UserId UserId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

    public GetUserStatisticsQuery(UserId userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        UserId = userId;
        FromDate = fromDate;
        ToDate = toDate;
    }
}