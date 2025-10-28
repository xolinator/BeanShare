using BeanShare.Application.Common;
using BeanShare.Application.Features.Billing.Dtos;
using MediatR;

namespace BeanShare.Application.Features.Billing.Queries.GetSpaceBillingPeriods;

public sealed record GetSpaceBillingPeriodsQuery(
    Guid SpaceId
) : IRequest<Result<List<BillingPeriodSummaryDto>>>;