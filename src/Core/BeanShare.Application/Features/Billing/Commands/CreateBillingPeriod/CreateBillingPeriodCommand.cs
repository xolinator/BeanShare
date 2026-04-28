using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Billing.Dtos;

namespace BeanShare.Application.Features.Billing.Commands.CreateBillingPeriod;
public sealed record CreateBillingPeriodCommand(
    Guid SpaceId,
    string Name,
    DateTime StartDate,
    DateTime? EndDate = null
) : ICommand<Result<BillingPeriodDto>>;