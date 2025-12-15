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
    string Status,
    DateTime? CompletedAt,
    int ConfirmedLinesCount,
    int TotalLinesCount,
    List<SettlementLineDto> Lines
);
