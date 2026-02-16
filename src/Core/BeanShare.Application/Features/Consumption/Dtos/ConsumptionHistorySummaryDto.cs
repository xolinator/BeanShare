namespace BeanShare.Application.Features.Consumption.Dtos;
public sealed record ConsumptionHistorySummaryDto(
    decimal TotalGrams,
    int TotalEntries,
    int UniqueDays,
    decimal AverageGramsPerDay,
    Dictionary<string, int> ConsumptionByType,
    Dictionary<string, decimal> ConsumptionGramsBySpace
);
