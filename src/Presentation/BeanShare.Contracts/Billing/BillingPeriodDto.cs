namespace BeanShare.Contracts.Billing;

public record BillingPeriodDto(
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
    string Currency
);

public record BillingPeriodSummaryDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string State,
    int DaysRemaining,
    int ConsumptionCount,
    decimal TotalCoffeeGrams
);

public record CreateBillingPeriodDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
