namespace BeanShare.Contracts.Settlement;

public record SettlementDto(
    Guid Id,
    Guid SpaceId,
    string SpaceName,
    Guid BillingPeriodId,
    string BillingPeriodName,
    DateTime BillingPeriodStartDate,
    DateTime BillingPeriodEndDate,
    DateTime GeneratedAt,
    decimal TotalAmount,
    string Currency,
    List<SettlementLineDto> Lines
);

public record SettlementLineDto(
    Guid UserId,
    string UserName,
    string UserEmail,
    decimal TotalCoffeeGrams,
    decimal? TotalMilkMl,
    decimal AmountDue,
    decimal ConsumptionPercentage
);

public record SettlementSummaryDto(
    Guid Id,
    Guid BillingPeriodId,
    string BillingPeriodName,
    DateTime GeneratedAt,
    decimal TotalAmount,
    string Currency,
    int ParticipantCount,
    decimal? UserShare
);
