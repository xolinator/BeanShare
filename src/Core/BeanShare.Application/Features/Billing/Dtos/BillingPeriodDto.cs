namespace BeanShare.Application.Features.Billing.Dtos;
public sealed record BillingPeriodDto(
    Guid Id,
    Guid SpaceId,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string State,
    DateTime CreatedAt,
    Guid CreatedBy,
    DateTime? OpenedAt,
    Guid? OpenedBy,
    DateTime? ClosedAt,
    Guid? ClosedBy,
    DateTime? SettledAt,
    Guid? SettledBy,
    int ConsumptionCount,
    decimal TotalCoffeeGrams,
    decimal? EstimatedCost,
    string? Currency
);
