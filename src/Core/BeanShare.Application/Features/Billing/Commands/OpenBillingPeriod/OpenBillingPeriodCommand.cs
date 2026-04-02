using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Billing.Commands.OpenBillingPeriod;
public sealed record OpenBillingPeriodCommand(Guid BillingPeriodId) : ICommand<Result>;