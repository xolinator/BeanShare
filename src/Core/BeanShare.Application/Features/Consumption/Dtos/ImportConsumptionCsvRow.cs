namespace BeanShare.Application.Features.Consumption.Dtos;

public sealed record ImportConsumptionCsvRow(
    int RowNumber,
    string Email,
    string ProductName,
    string ProductBrand,
    string ProductType,
    decimal QuantityGrams,
    DateTime ConsumedAt
);
