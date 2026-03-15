namespace BeanShare.Contracts.Consumption;

public sealed record ImportConsumptionCsvRequest
{
    public required Guid SpaceId { get; init; }
    public required IReadOnlyList<ImportConsumptionCsvRowRequest> Rows { get; init; }
}

public sealed record ImportConsumptionCsvRowRequest
{
    public required int RowNumber { get; init; }
    public required string Email { get; init; }
    public required string ProductName { get; init; }
    public required string ProductBrand { get; init; }
    public required string ProductType { get; init; }
    public required decimal QuantityGrams { get; init; }
    public required DateTime ConsumedAt { get; init; }
}
