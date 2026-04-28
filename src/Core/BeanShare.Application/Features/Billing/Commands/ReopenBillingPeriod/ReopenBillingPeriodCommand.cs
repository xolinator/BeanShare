using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Billing.Commands.ReopenBillingPeriod;
public sealed record ReopenBillingPeriodCommand(Guid BillingPeriodId) : ICommand<Result>;
