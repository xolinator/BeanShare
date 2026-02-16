using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Exceptions;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.CoffeeStock.Commands;
public sealed class ConsumeStockCommandHandler : IRequestHandler<ConsumeStockCommand, Result<ConsumedStockDto>>
{
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public ConsumeStockCommandHandler(
        ICoffeeStockRepository coffeeStockRepository,
        IUserContext userContext,
        IClock clock)
    {
        _coffeeStockRepository = coffeeStockRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<ConsumedStockDto>> Handle(ConsumeStockCommand request, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        if (!Enum.TryParse<Domain.ValueObjects.CoffeeType>(request.ProductType, true, out var coffeeType))
        {
            return Result<ConsumedStockDto>.Failure(Error.InvalidProductType());
        }
        var product = CoffeeProduct.Create(request.ProductName, request.ProductBrand, coffeeType);
        var quantity = Weight.FromGrams(request.QuantityGrams);

        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        if (coffeeStock == null)
        {
            return Result<ConsumedStockDto>.Failure(Error.StockNotFound(request.SpaceId));
        }

        try
        {
            coffeeStock.ConsumeStock(product, quantity, _clock);
            await _coffeeStockRepository.UpdateAsync(coffeeStock, cancellationToken);
            var remaining = coffeeStock.GetCurrentStock(product).Grams;
            return Result<ConsumedStockDto>.Success(new ConsumedStockDto(
                request.SpaceId,
                request.ProductName,
                request.ProductBrand,
                request.ProductType,
                request.QuantityGrams,
                remaining));
        }
        catch (StockDomainException ex) when (ex.Message.Contains("not found"))
        {
            return Result<ConsumedStockDto>.Failure(Error.ProductNotFoundInStock());
        }
        catch (StockDomainException ex) when (ex.Message.Contains("Cannot consume"))
        {
            return Result<ConsumedStockDto>.Failure(Error.InsufficientStock(ex.Message));
        }
    }
}