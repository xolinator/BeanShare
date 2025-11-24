namespace BeanShare.Application.Features.Billing.Dtos;

public sealed record CreateBillingPeriodDto(
    string Name,
    DateTime StartDate,
    DateTime EndDate
);
