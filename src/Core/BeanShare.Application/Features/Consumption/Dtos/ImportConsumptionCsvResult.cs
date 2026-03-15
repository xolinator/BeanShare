namespace BeanShare.Application.Features.Consumption.Dtos;

public sealed record ImportConsumptionCsvResult
{
    public required int TotalRows { get; init; }
    public required int SuccessCount { get; init; }
    public required int ErrorCount { get; init; }
    public required IReadOnlyList<ImportRowError> Errors { get; init; }
}

public sealed record ImportRowError(int RowNumber, string Field, string Message);
