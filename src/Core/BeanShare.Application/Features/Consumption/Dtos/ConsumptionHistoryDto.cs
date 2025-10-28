namespace BeanShare.Application.Features.Consumption.Dtos;

public sealed record ConsumptionHistoryDto(
    int TotalCount,
    int PageNumber,
    int PageSize,
    List<ConsumptionHistoryItemDto> Items,
    ConsumptionHistorySummaryDto Summary
);

public sealed record ConsumptionHistoryItemDto(
    Guid Id,
    Guid SpaceId,
    string SpaceName,
    string ProductName,
    string ProductBrand,
    string ProductType,
    decimal QuantityGrams,
    DateTime ConsumedAt,
    decimal? EstimatedCost,
    string? Currency,
    Guid? BillingPeriodId,
    string? BillingPeriodName
);

public sealed record ConsumptionHistorySummaryDto(
    decimal TotalGrams,
    int TotalEntries,
    int UniqueDays,
    decimal AverageGramsPerDay,
    Dictionary<string, int> ConsumptionByType,
    Dictionary<string, decimal> ConsumptionGramsBySpace
);