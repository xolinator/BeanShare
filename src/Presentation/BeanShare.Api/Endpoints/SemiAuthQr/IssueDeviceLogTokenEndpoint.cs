using BeanShare.Api.Endpoints.Common;
using BeanShare.Application.Features.SemiAuthQr.Commands;
using BeanShare.Contracts.SemiAuthQr;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.SemiAuthQr;

public sealed class IssueDeviceLogTokenEndpoint(IMediator mediator)
    : Endpoint<IssueDeviceLogTokenRequest, DeviceLogTokenResponse>
{
    public override void Configure()
    {
        Post("/api/me/semi-auth-qr/device-token");
        Summary(s =>
        {
            s.Summary = "Issue or rotate semi-auth QR device token";
            s.Description = "Issues a device-bound token used for anonymous QR consumption logging.";
        });
    }

    public override async Task HandleAsync(IssueDeviceLogTokenRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new IssueSemiAuthQrDeviceTokenCommand(req.DeviceId), ct);
        if (result.IsFailure)
        {
            await this.SendResultErrorsAsync(result, ct);
            return;
        }

        var dto = result.Value;
        await SendOkAsync(new DeviceLogTokenResponse
        {
            TokenId = dto.TokenId,
            UserId = dto.UserId,
            DeviceId = dto.DeviceId,
            Token = dto.Token,
            IssuedAt = dto.IssuedAt,
            ExpiresAt = dto.ExpiresAt
        }, ct);
    }
}
