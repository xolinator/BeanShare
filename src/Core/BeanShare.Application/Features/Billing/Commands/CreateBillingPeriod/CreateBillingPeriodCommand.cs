using BeanShare.Application.Common;
using BeanShare.Application.Features.Billing.Dtos;
using MediatR;

namespace BeanShare.Application.Features.Billing.Commands.CreateBillingPeriod;
public sealed record CreateBillingPeriodCommand(
    Guid SpaceId,
    string Name,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<Result<BillingPeriodDto>>;