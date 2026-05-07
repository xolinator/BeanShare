using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Exceptions;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeanShare.Application.Features.SemiAuthQr.Commands;

public sealed class RecordSemiAuthQrConsumptionCommandHandler(
    IActiveQrCodeRepository activeQrCodeRepository,
    ISemiAuthQrDeviceTokenRepository tokenRepository,
    IUserRepository userRepository,
    ISpaceRepository spaceRepository,
    ICoffeeStockRepository coffeeStockRepository,
    IConsumptionRepository consumptionRepository,
    ISemiAuthQrSettings settings,
    IClock clock,
    ILogger<RecordSemiAuthQrConsumptionCommandHandler> logger)
    : IRequestHandler<RecordSemiAuthQrConsumptionCommand, Result<ConsumptionEntryDto>>
{
    public async Task<Result<ConsumptionEntryDto>> Handle(RecordSemiAuthQrConsumptionCommand request, CancellationToken cancellationToken)
    {
        if (!settings.Enabled)
            return Result<ConsumptionEntryDto>.Failure(Error.Forbidden("SemiAuthQr", "Semi-auth QR logging is disabled"));

        if (!Guid.TryParse(request.DeviceId, out var deviceId) || deviceId == Guid.Empty)
            return Result<ConsumptionEntryDto>.Failure(Error.InvalidRequest(nameof(request.DeviceId)));

        if (string.IsNullOrWhiteSpace(request.DeviceToken))
            return Result<ConsumptionEntryDto>.Failure(Error.Unauthenticated());

        var deviceHash = SemiAuthQrCrypto.Hash(request.DeviceId);
        var tokenHash = SemiAuthQrCrypto.Hash(request.DeviceToken);
        var token = await tokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);
        if (token == null)
            return Result<ConsumptionEntryDto>.Failure(Error.Unauthenticated());

        if (!SemiAuthQrCrypto.SecureEquals(token.DeviceIdHash, deviceHash))
        {
            logger.LogWarning(
                "Semi-auth QR token device mismatch (tokenId={TokenId}, userId={UserId}, qrCodeId={QrCodeId}, at={TimestampUtc})",
                token.Id,
                token.UserId.Value,
                request.QrCodeId,
                clock.UtcNow);
            return Result<ConsumptionEntryDto>.Failure(Error.Unauthenticated());
        }

        if (!token.CanBeUsed(clock.UtcNow))
            return Result<ConsumptionEntryDto>.Failure(Error.Unauthenticated());

        var user = await userRepository.GetByIdAsync(token.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            token.Revoke("user_inactive_or_missing", clock);
            await tokenRepository.UpdateAsync(token, cancellationToken);
            return Result<ConsumptionEntryDto>.Failure(Error.Unauthenticated());
        }

        var qrSpec = new ActiveQrCodeByIdSpecification(new ActiveQrCodeId(request.QrCodeId));
        var qrCode = await activeQrCodeRepository.GetSingleBySpecAsync(qrSpec, cancellationToken);
        if (qrCode == null || !qrCode.IsActive)
            return Result<ConsumptionEntryDto>.Failure(Error.NotFound("ActiveQrCode", $"QR code {request.QrCodeId} not found or inactive"));

        var space = await spaceRepository.GetByIdAsync(qrCode.SpaceId, cancellationToken);
        if (space == null)
            return Result<ConsumptionEntryDto>.Failure(Error.SpaceNotFound(qrCode.SpaceId.Value));

        if (!space.HasMember(token.UserId))
        {
            token.Revoke("space_membership_removed", clock);
            await tokenRepository.UpdateAsync(token, cancellationToken);
            return Result<ConsumptionEntryDto>.Failure(Error.NotMember());
        }

        var grams = request.QuantityGrams ?? qrCode.DefaultGrams;
        if (grams <= 0)
            return Result<ConsumptionEntryDto>.Failure(Error.ValidationFailure(nameof(request.QuantityGrams), "QuantityGrams must be provided and greater than 0 for open-recipe QR codes."));

        var consumedAt = request.ConsumedAt ?? clock.UtcNow;
        if (consumedAt > clock.UtcNow.AddMinutes(5))
            return Result<ConsumptionEntryDto>.Failure(Error.InvalidConsumptionTime());

        var coffeeStock = await coffeeStockRepository.GetBySpaceIdAsync(qrCode.SpaceId, cancellationToken);
        if (coffeeStock == null)
            return Result<ConsumptionEntryDto>.Failure(Error.StockNotFound(qrCode.SpaceId.Value));

        try
        {
            var quantity = Weight.FromGrams(grams);
            coffeeStock.ConsumeStock(qrCode.Product, quantity, clock);
            await coffeeStockRepository.UpdateAsync(coffeeStock, cancellationToken);

            var remainingStock = coffeeStock.GetCurrentStock(qrCode.Product);
            var presetName = qrCode.DefaultGrams > 0 && !string.IsNullOrWhiteSpace(qrCode.RecipeName)
                ? qrCode.RecipeName
                : null;

            var entry = Domain.Entities.ConsumptionEntry.Create(
                qrCode.SpaceId,
                token.UserId,
                qrCode.Product,
                quantity,
                consumedAt,
                clock,
                presetId: null,
                presetName: presetName);

            await consumptionRepository.AddAsync(entry, cancellationToken);

            token.MarkUsed(clock.UtcNow, qrCode.SpaceId, qrCode.Id);
            await tokenRepository.UpdateAsync(token, cancellationToken);

            logger.LogInformation(
                "Semi-auth QR consumption recorded (tokenId={TokenId}, userId={UserId}, spaceId={SpaceId}, qrCodeId={QrCodeId}, deviceHash={DeviceHash}, consumedAt={ConsumedAtUtc})",
                token.Id,
                token.UserId.Value,
                qrCode.SpaceId.Value,
                qrCode.Id.Value,
                token.DeviceIdHash,
                consumedAt);

            return Result<ConsumptionEntryDto>.Success(new ConsumptionEntryDto
            {
                Id = entry.Id.Value,
                SpaceId = entry.SpaceId.Value,
                UserId = entry.UserId.Value,
                ProductName = entry.Product.Name,
                ProductBrand = entry.Product.Brand,
                ProductType = entry.Product.Type.ToString(),
                QuantityGrams = entry.Quantity.Grams,
                RemainingGrams = remainingStock.Grams,
                ConsumedAt = entry.ConsumedAt,
                CreatedAt = entry.CreatedAt,
                PresetName = presetName
            });
        }
        catch (ProductNotFoundException)
        {
            return Result<ConsumptionEntryDto>.Failure(Error.ProductNotFoundInStock());
        }
        catch (InsufficientStockException ex)
        {
            return Result<ConsumptionEntryDto>.Failure(Error.InsufficientStock(ex.Message));
        }
    }
}
