using BeanShare.Application.Abstractions;
using BeanShare.Application.Features.Billing.Dtos;
using BeanShare.Application.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Billing.Queries.GetBillingPeriodById;

public sealed record GetBillingPeriodByIdQuery(BillingPeriodId Id) : IQuery<Result<BillingPeriodDto>>;