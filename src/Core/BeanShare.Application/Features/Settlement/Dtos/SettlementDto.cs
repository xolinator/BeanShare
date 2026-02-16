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
    string Status,
    DateTime? CompletedAt,
    int ConfirmedLinesCount,
    int TotalLinesCount,
    List<SettlementLineDto> Lines
);
