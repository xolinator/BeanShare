using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Exceptions;
using BeanShare.Domain.Repositories;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Consumption.Commands;
public sealed class RecordConsumptionFromPresetCommandHandler : IRequestHandler<RecordConsumptionFromPresetCommand, Result<ConsumptionEntryDto>>
{
    private readonly IPresetRecipeRepository _presetRepository;
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public RecordConsumptionFromPresetCommandHandler(
        IPresetRecipeRepository presetRepository,
        ICoffeeStockRepository coffeeStockRepository,
        IConsumptionRepository consumptionRepository,
        IUserContext userContext,
        IClock clock)
    {
        _presetRepository = presetRepository;
        _coffeeStockRepository = coffeeStockRepository;
        _consumptionRepository = consumptionRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<ConsumptionEntryDto>> Handle(RecordConsumptionFromPresetCommand request, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var userId = request.ForUserId.HasValue
            ? new UserId(request.ForUserId.Value)
            : _userContext.CurrentUserId;
        var presetId = new PresetRecipeId(request.PresetId);

        var preset = await _presetRepository.GetByIdAsync(presetId, cancellationToken);
        if (preset is null)
        {
            return Result<ConsumptionEntryDto>.Failure(Error.NotFound("PresetRecipe", "Preset not found"));
        }

        if (preset.SpaceId != spaceId)
        {
            return Result<ConsumptionEntryDto>.Failure(Error.Forbidden("PresetRecipe", "Preset does not belong to this space"));
        }

        if (preset.UserId != userId && !preset.IsShared)
        {
            return Result<ConsumptionEntryDto>.Failure(Error.Forbidden("PresetRecipe", "You do not have access to this preset"));
        }

        if (!Enum.TryParse<Domain.ValueObjects.CoffeeType>(preset.CoffeeType, true, out var coffeeType))
        {
            return Result<ConsumptionEntryDto>.Failure(Error.InvalidCoffeeTypeInPreset());
        }

        var quantityGrams = request.CustomQuantityGrams ?? preset.DefaultGrams.Grams;
        var product = CoffeeProduct.Create(preset.Name, preset.Name, coffeeType);
        var quantity = Weight.FromGrams(quantityGrams);
        var consumedAt = request.ConsumedAt ?? _clock.UtcNow;

        if (consumedAt > _clock.UtcNow.AddMinutes(5))
        {
            return Result<ConsumptionEntryDto>.Failure(Error.InvalidConsumptionTime());
        }

        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        if (coffeeStock == null)
        {
            return Result<ConsumptionEntryDto>.Failure(Error.StockNotFound(request.SpaceId));
        }

        try
        {
            coffeeStock.ConsumeStock(product, quantity, _clock);
            await _coffeeStockRepository.UpdateAsync(coffeeStock, cancellationToken);

            var remainingStock = coffeeStock.GetCurrentStock(product);

            var consumptionEntry = ConsumptionEntry.Create(
                spaceId,
                userId,
                product,
                quantity,
                consumedAt,
                _clock,
                presetId,
                preset.Name);

            await _consumptionRepository.AddAsync(consumptionEntry, cancellationToken);

            preset.RecordUsage(_clock.UtcNow);
            await _presetRepository.UpdateAsync(preset, cancellationToken);

            var dto = new ConsumptionEntryDto
            {
                Id = consumptionEntry.Id.Value,
                SpaceId = consumptionEntry.SpaceId.Value,
                UserId = consumptionEntry.UserId.Value,
                ProductName = consumptionEntry.Product.Name,
                ProductBrand = consumptionEntry.Product.Brand,
                ProductType = consumptionEntry.Product.Type.ToString(),
                QuantityGrams = consumptionEntry.Quantity.Grams,
                RemainingGrams = remainingStock.Grams,
                ConsumedAt = consumptionEntry.ConsumedAt,
                CreatedAt = consumptionEntry.CreatedAt,
                PresetId = preset.Id.Value,
                PresetName = preset.Name
            };

            return Result<ConsumptionEntryDto>.Success(dto);
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