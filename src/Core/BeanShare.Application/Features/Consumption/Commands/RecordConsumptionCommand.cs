using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;

namespace BeanShare.Application.Features.Consumption.Commands;

public sealed record RecordConsumptionCommand(
    Guid SpaceId,
    string ProductName,
    string ProductBrand,
    string ProductType,
    decimal QuantityGrams,
    DateTime? ConsumedAt
) : ICommand<Result<ConsumptionEntryDto>>;