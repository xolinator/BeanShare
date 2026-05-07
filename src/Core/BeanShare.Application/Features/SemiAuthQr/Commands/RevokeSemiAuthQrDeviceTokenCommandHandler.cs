using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using MediatR;

namespace BeanShare.Application.Features.SemiAuthQr.Commands;

public sealed class RevokeSemiAuthQrDeviceTokenCommandHandler(
    IUserContext userContext,
    ISemiAuthQrDeviceTokenRepository tokenRepository,
    ISemiAuthQrSettings settings,
    IClock clock)
    : IRequestHandler<RevokeSemiAuthQrDeviceTokenCommand, Result>
{
    public async Task<Result> Handle(RevokeSemiAuthQrDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        if (!settings.Enabled)
            return Result.Success();

        if (!Guid.TryParse(request.DeviceId, out var deviceId) || deviceId == Guid.Empty)
            return Result.Success();

        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Success();

        var userId = userContext.CurrentUserId;
        var deviceHash = SemiAuthQrCrypto.Hash(request.DeviceId);
        var tokenHash = SemiAuthQrCrypto.Hash(request.Token);

        var existing = await tokenRepository.GetByUserAndDeviceAsync(userId, deviceHash, cancellationToken);
        if (existing == null)
            return Result.Success();

        if (!SemiAuthQrCrypto.SecureEquals(existing.TokenHash, tokenHash))
            return Result.Success();

        existing.Revoke("logout", clock);
        await tokenRepository.UpdateAsync(existing, cancellationToken);
        return Result.Success();
    }
}
