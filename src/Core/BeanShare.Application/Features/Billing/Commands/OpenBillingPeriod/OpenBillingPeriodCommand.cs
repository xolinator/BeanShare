using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Billing.Commands.OpenBillingPeriod;
public sealed record OpenBillingPeriodCommand(Guid BillingPeriodId) : IRequest<Result>;