namespace BeanShare.Application.Features.Billing.Dtos;

public sealed record BillingPeriodSummaryDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string State,
    int DaysRemaining,
    int ConsumptionCount,
    decimal TotalCoffeeGrams
);
