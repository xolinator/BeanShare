using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;
using MediatR;

namespace BeanShare.Application.Features.Consumption.Queries.GetUserConsumptionHistory;

public sealed record GetUserConsumptionHistoryQuery(
    Guid? SpaceId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    Guid? BillingPeriodId = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<ConsumptionHistoryDto>>;