using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Consumption.Commands;

public sealed class UpdateConsumptionCommandHandler
    : IRequestHandler<UpdateConsumptionCommand, Result<ConsumptionEntryDto>>
{
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly IBillingPeriodRepository _billingPeriodRepository;
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly IClock _clock;

    public UpdateConsumptionCommandHandler(
        IConsumptionRepository consumptionRepository,
        IBillingPeriodRepository billingPeriodRepository,
        ICoffeeStockRepository coffeeStockRepository,
        IClock clock)
    {
        _consumptionRepository = consumptionRepository;
        _billingPeriodRepository = billingPeriodRepository;
        _coffeeStockRepository = coffeeStockRepository;
        _clock = clock;
    }

    public async Task<Result<ConsumptionEntryDto>> Handle(
        UpdateConsumptionCommand request,
        CancellationToken cancellationToken)
    {
        var entryId = new ConsumptionEntryId(request.Id);
        var entry = await _consumptionRepository.GetByIdAsync(entryId, cancellationToken);

        if (entry == null)
            return Result<ConsumptionEntryDto>.Failure(
                Error.NotFound("ConsumptionEntry", $"Consumption entry {request.Id} not found"));

        if (entry.SpaceId.Value != request.SpaceId)
            return Result<ConsumptionEntryDto>.Failure(Error.InsufficientSpacePrivileges("edit this consumption"));

        if (entry.BillingPeriodId != null)
        {
            var billingPeriod = await _billingPeriodRepository.GetByIdAsync(entry.BillingPeriodId.Value, cancellationToken);
            if (billingPeriod != null && billingPeriod.State != BillingState.Draft && billingPeriod.State != BillingState.Open)
                return Result<ConsumptionEntryDto>.Failure(
                    Error.InvalidBillingPeriodState("edit consumption in", billingPeriod.State.ToString()));
        }

        if (!Enum.TryParse<CoffeeType>(request.ProductType, ignoreCase: true, out var coffeeType))
            return Result<ConsumptionEntryDto>.Failure(Error.InvalidCoffeeType());

        var product = CoffeeProduct.Create(request.ProductName.Trim(), request.ProductBrand.Trim(), coffeeType);
        var quantity = Weight.FromGrams(request.QuantityGrams);

        var oldProduct = entry.Product;
        var oldQuantity = entry.Quantity;

        try
        {
            entry.Update(product, quantity, request.ConsumedAt, _clock);
        }
        catch (ArgumentException ex)
        {
            return Result<ConsumptionEntryDto>.Failure(Error.DomainError(ex.Message));
        }

        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(entry.SpaceId, cancellationToken);
        if (coffeeStock != null)
        {
            coffeeStock.RestoreStock(oldProduct, oldQuantity, _clock);
            coffeeStock.ConsumeStock(product, quantity, _clock);
            await _coffeeStockRepository.UpdateAsync(coffeeStock, cancellationToken);
        }

        await _consumptionRepository.UpdateAsync(entry, cancellationToken);

        var dto = new ConsumptionEntryDto
        {
            Id = entry.Id.Value,
            SpaceId = entry.SpaceId.Value,
            UserId = entry.UserId.Value,
            ProductName = entry.Product.Name,
            ProductBrand = entry.Product.Brand,
            ProductType = entry.Product.Type.ToString(),
            QuantityGrams = entry.Quantity.Grams,
            RemainingGrams = 0,
            ConsumedAt = entry.ConsumedAt,
            CreatedAt = entry.CreatedAt,
            CanEdit = true,
            PresetId = entry.PresetId?.Value,
            PresetName = entry.PresetName
        };

        return Result<ConsumptionEntryDto>.Success(dto);
    }
}
