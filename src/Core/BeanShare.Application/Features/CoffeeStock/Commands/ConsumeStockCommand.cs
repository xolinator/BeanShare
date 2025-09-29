using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed record ConsumeStockCommand(
    Guid SpaceId,
    string ProductName,
    string ProductBrand,
    string ProductType,
    decimal QuantityGrams
) : ICommand<Result<ConsumedStockDto>>;

public sealed record ConsumedStockDto(
    Guid SpaceId,
    string ProductName,
    string ProductBrand,
    string ProductType,
    decimal ConsumedGrams,
    decimal RemainingGrams);