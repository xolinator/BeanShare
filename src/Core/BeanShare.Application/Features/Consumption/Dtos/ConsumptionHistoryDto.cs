namespace BeanShare.Application.Features.Consumption.Dtos;
public sealed record ConsumptionHistoryDto(
    int TotalCount,
    int PageNumber,
    int PageSize,
    List<ConsumptionHistoryItemDto> Items,
    ConsumptionHistorySummaryDto Summary
);
