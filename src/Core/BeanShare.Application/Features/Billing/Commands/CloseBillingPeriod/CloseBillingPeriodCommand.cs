using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Billing.Commands.CloseBillingPeriod;
public sealed record CloseBillingPeriodCommand(Guid BillingPeriodId) : ICommand<Result>;