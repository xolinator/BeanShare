using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.CoffeeStock.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed class UpdateStockPurchaseCommandHandler
    : IRequestHandler<UpdateStockPurchaseCommand, Result<StockPurchaseDto>>
{
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public UpdateStockPurchaseCommandHandler(
        ICoffeeStockRepository coffeeStockRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IClock clock)
    {
        _coffeeStockRepository = coffeeStockRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<StockPurchaseDto>> Handle(
        UpdateStockPurchaseCommand command,
        CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.CurrentUserId;
        var spaceId = new SpaceId(command.SpaceId);

        var spaceSpec = new SpaceByIdSpecification(spaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);

        if (space == null)
            return Result<StockPurchaseDto>.Failure(Error.SpaceNotFound(command.SpaceId));

        if (!space.IsAdmin(currentUserId))
            return Result<StockPurchaseDto>.Failure(Error.InsufficientSpacePrivileges("edit stock purchases"));

        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        if (coffeeStock == null)
            return Result<StockPurchaseDto>.Failure(Error.StockNotFound(command.SpaceId));

        try
        {
            coffeeStock.UpdatePurchase(
                command.PurchaseId,
                Weight.FromGrams(command.QuantityGrams),
                command.CostAmount,
                command.PurchasedAt,
                _clock);

            await _coffeeStockRepository.UpdateAsync(coffeeStock, cancellationToken);

            var purchase = coffeeStock.Purchases.First(p => p.Id == command.PurchaseId);
            var dto = new StockPurchaseDto
            {
                Id = purchase.Id,
                ProductName = purchase.Product.Name,
                ProductBrand = purchase.Product.Brand,
                ProductType = purchase.Product.Type.ToString(),
                QuantityGrams = purchase.Quantity.Grams,
                CostAmount = purchase.Cost.Amount,
                CostCurrency = purchase.Cost.Currency,
                Vendor = purchase.Vendor,
                PurchasedBy = purchase.PurchasedBy.Value,
                PurchasedAt = purchase.PurchasedAt,
                CreatedAt = purchase.CreatedAt,
                CostPerGram = purchase.CostPerGram.Amount
            };

            return Result<StockPurchaseDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Result<StockPurchaseDto>.Failure(Error.DomainError(ex.Message));
        }
        catch (ArgumentException ex)
        {
            return Result<StockPurchaseDto>.Failure(Error.ValidationFailure(nameof(UpdateStockPurchaseCommand), ex.Message));
        }
    }
}
