using BeanShare.Api.Endpoints.Common;
using BeanShare.Application.Features.SemiAuthQr.Commands;
using BeanShare.Contracts.Consumption;
using BeanShare.Contracts.SemiAuthQr;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.SemiAuthQr;

public sealed class RecordSemiAuthQrConsumptionEndpoint(IMediator mediator)
    : Endpoint<RecordSemiAuthQrConsumptionRequest, RecordConsumptionResponse>
{
    public override void Configure()
    {
        Post("/api/qr-codes/{qrCodeId}/semi-auth-consumptions");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Record consumption via semi-auth QR token";
            s.Description = "Anonymous endpoint that logs consumption only when a valid device token was issued from a prior authenticated session on the same device.";
        });
    }

    public override async Task HandleAsync(RecordSemiAuthQrConsumptionRequest req, CancellationToken ct)
    {
        var qrCodeId = Route<Guid>("qrCodeId");
        var command = new RecordSemiAuthQrConsumptionCommand(
            qrCodeId,
            req.DeviceId,
            req.DeviceToken,
            req.QuantityGrams,
            req.ConsumedAt);

        var result = await mediator.Send(command, ct);
        if (result.IsFailure)
        {
            await this.SendResultErrorsAsync(result, ct);
            return;
        }

        var dto = result.Value;
        await SendAsync(new RecordConsumptionResponse
        {
            SpaceId = dto.SpaceId,
            ProductName = dto.ProductName,
            ProductBrand = dto.ProductBrand,
            ProductType = dto.ProductType,
            ConsumedGrams = dto.QuantityGrams,
            RemainingGrams = dto.RemainingGrams,
            ConsumedAt = dto.ConsumedAt
        }, 201, ct);
    }
}
