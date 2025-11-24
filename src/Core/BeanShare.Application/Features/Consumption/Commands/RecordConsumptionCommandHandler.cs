using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Exceptions;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Consumption.Commands;

public sealed class RecordConsumptionCommandHandler : IRequestHandler<RecordConsumptionCommand, Result<ConsumptionEntryDto>>
{
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public RecordConsumptionCommandHandler(
        ICoffeeStockRepository coffeeStockRepository,
        IConsumptionRepository consumptionRepository,
        IUserContext userContext,
        IClock clock)
    {
        _coffeeStockRepository = coffeeStockRepository;
        _consumptionRepository = consumptionRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<ConsumptionEntryDto>> Handle(RecordConsumptionCommand request, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var userId = _userContext.CurrentUserId;

        if (!Enum.TryParse<Domain.ValueObjects.CoffeeType>(request.ProductType, true, out var coffeeType))
        {
            return Result<ConsumptionEntryDto>.Failure(new Error("consumption.invalid_type", "Invalid coffee type"));
        }

        var product = CoffeeProduct.Create(request.ProductName, request.ProductBrand, coffeeType);
        var quantity = Weight.FromGrams(request.QuantityGrams);
        var consumedAt = request.ConsumedAt ?? _clock.UtcNow;

        if (consumedAt > _clock.UtcNow)
        {
            return Result<ConsumptionEntryDto>.Failure(new Error("consumption.invalid_time", "Consumption time cannot be in the future"));
        }

        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        if (coffeeStock == null)
        {
            return Result<ConsumptionEntryDto>.Failure(new Error("stock.not_found", "No coffee stock found for this space"));
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
                _clock);

            await _consumptionRepository.AddAsync(consumptionEntry, cancellationToken);

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
                PresetName = request.PresetName
            };

            return Result<ConsumptionEntryDto>.Success(dto);
        }
        catch (ProductNotFoundException)
        {
            return Result<ConsumptionEntryDto>.Failure(new Error("stock.product_not_found", "Product not found in stock"));
        }
        catch (InsufficientStockException ex)
        {
            return Result<ConsumptionEntryDto>.Failure(new Error("stock.insufficient", ex.Message));
        }
    }
}