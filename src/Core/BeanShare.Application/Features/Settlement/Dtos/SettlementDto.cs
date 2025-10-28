namespace BeanShare.Application.Features.Settlement.Dtos;

public sealed record SettlementDto(
    Guid Id,
    Guid SpaceId,
    Guid BillingPeriodId,
    string BillingPeriodName,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    DateTime GeneratedAt,
    Guid GeneratedBy,
    decimal TotalAmount,
    string Currency,
    List<SettlementLineDto> Lines
);

public sealed record SettlementLineDto(
    Guid UserId,
    string UserName,
    string UserEmail,
    decimal TotalCoffeeGrams,
    decimal? TotalMilkMl,
    decimal AmountDue,
    string Currency,
    decimal ConsumptionPercentage
);

public sealed record SettlementSummaryDto(
    Guid Id,
    string BillingPeriodName,
    DateTime GeneratedAt,
    decimal TotalAmount,
    string Currency,
    int UserCount,
    decimal YourShare
);