using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.SemiAuthQr.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using MediatR;

namespace BeanShare.Application.Features.SemiAuthQr.Commands;

public sealed class IssueSemiAuthQrDeviceTokenCommandHandler(
    IUserContext userContext,
    IUserRepository userRepository,
    ISemiAuthQrDeviceTokenRepository tokenRepository,
    ISemiAuthQrSettings settings,
    IClock clock)
    : IRequestHandler<IssueSemiAuthQrDeviceTokenCommand, Result<DeviceLogTokenDto>>
{
    public async Task<Result<DeviceLogTokenDto>> Handle(IssueSemiAuthQrDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        if (!settings.Enabled)
            return Result<DeviceLogTokenDto>.Failure(Error.Forbidden("SemiAuthQr", "Semi-auth QR logging is disabled"));

        if (!Guid.TryParse(request.DeviceId, out var deviceId) || deviceId == Guid.Empty)
            return Result<DeviceLogTokenDto>.Failure(Error.InvalidRequest(nameof(request.DeviceId)));

        var userId = userContext.CurrentUserId;
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsActive)
            return Result<DeviceLogTokenDto>.Failure(Error.Unauthenticated());

        var issuedAt = clock.UtcNow;
        var expiresAt = issuedAt.Add(settings.DeviceTokenLifetime);
        var deviceHash = SemiAuthQrCrypto.Hash(request.DeviceId);
        var rawToken = SemiAuthQrCrypto.CreateRawToken();
        var tokenHash = SemiAuthQrCrypto.Hash(rawToken);

        var existing = await tokenRepository.GetByUserAndDeviceAsync(userId, deviceHash, cancellationToken);
        if (existing == null)
        {
            var tokenEntity = SemiAuthQrDeviceToken.Create(userId, deviceHash, tokenHash, expiresAt, clock);
            await tokenRepository.AddAsync(tokenEntity, cancellationToken);

            return Result<DeviceLogTokenDto>.Success(new DeviceLogTokenDto
            {
                TokenId = tokenEntity.Id,
                UserId = userId.Value,
                DeviceId = request.DeviceId,
                Token = rawToken,
                IssuedAt = issuedAt,
                ExpiresAt = expiresAt
            });
        }

        existing.Rotate(tokenHash, expiresAt, clock);
        await tokenRepository.UpdateAsync(existing, cancellationToken);

        return Result<DeviceLogTokenDto>.Success(new DeviceLogTokenDto
        {
            TokenId = existing.Id,
            UserId = userId.Value,
            DeviceId = request.DeviceId,
            Token = rawToken,
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt
        });
    }
}
