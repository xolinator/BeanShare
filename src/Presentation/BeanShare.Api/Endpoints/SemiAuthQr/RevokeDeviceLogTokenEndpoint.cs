using BeanShare.Api.Endpoints.Common;
using BeanShare.Application.Features.SemiAuthQr.Commands;
using BeanShare.Contracts.SemiAuthQr;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.SemiAuthQr;

public sealed class RevokeDeviceLogTokenEndpoint(IMediator mediator)
    : Endpoint<RevokeDeviceLogTokenRequest>
{
    public override void Configure()
    {
        Post("/api/me/semi-auth-qr/device-token/revoke");
        Summary(s =>
        {
            s.Summary = "Revoke semi-auth QR device token";
            s.Description = "Revokes the current device token, typically on logout.";
        });
    }

    public override async Task HandleAsync(RevokeDeviceLogTokenRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new RevokeSemiAuthQrDeviceTokenCommand(req.DeviceId, req.Token), ct);
        if (result.IsFailure)
        {
            await this.SendResultErrorsAsync(result, ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}
