using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Billing.Commands.CloseBillingPeriod;
public sealed record CloseBillingPeriodCommand(Guid BillingPeriodId) : IRequest<Result>;