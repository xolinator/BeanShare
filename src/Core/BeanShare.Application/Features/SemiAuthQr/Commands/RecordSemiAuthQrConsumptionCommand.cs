using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;

namespace BeanShare.Application.Features.SemiAuthQr.Commands;

public sealed record RecordSemiAuthQrConsumptionCommand(
    Guid QrCodeId,
    string DeviceId,
    string DeviceToken,
    decimal? QuantityGrams = null,
    DateTime? ConsumedAt = null
) : ICommand<Result<ConsumptionEntryDto>>;
