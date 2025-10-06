using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.CoffeeStock.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MapsterMapper;
using MediatR;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed class AddStockPurchaseCommandHandler : IRequestHandler<AddStockPurchaseCommand, Result<StockPurchaseDto>>
{
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public AddStockPurchaseCommandHandler(
        ICoffeeStockRepository coffeeStockRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IClock clock,
        IMapper mapper)
    {
        _coffeeStockRepository = coffeeStockRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _clock = clock;
        _mapper = mapper;
    }

    public async Task<Result<StockPurchaseDto>> Handle(AddStockPurchaseCommand command, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(command.SpaceId);
        var spaceSpec = new SpaceByIdSpecification(spaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);

        if (space == null)
        {
            return Result<StockPurchaseDto>.Failure(Error.SpaceNotFound(command.SpaceId));
        }

        var currentUserId = _userContext.CurrentUserId;
        if (!space.IsAdmin(currentUserId))
        {
            return Result<StockPurchaseDto>.Failure(Error.InsufficientSpacePrivileges("manage coffee stock"));
        }

        try
        {
            var product = CoffeeProduct.Create(
                command.ProductName,
                command.ProductBrand,
                Enum.Parse<Domain.ValueObjects.CoffeeType>(command.ProductType));

            var quantity = Weight.FromGrams(command.QuantityGrams);
            var cost = Money.Create(command.CostAmount, command.CostCurrency);

            var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
            bool isNewStock = false;

            if (coffeeStock == null)
            {
                coffeeStock = Domain.Aggregates.CoffeeStock.CoffeeStock.Create(spaceId, _clock);
                await _coffeeStockRepository.AddAsync(coffeeStock, cancellationToken);
                isNewStock = true;
            }
            else
            {
                var existingPurchases = coffeeStock.Purchases.ToList();
                if (existingPurchases.Any() && existingPurchases.First().Cost.Currency != command.CostCurrency)
                {
                    return Result<StockPurchaseDto>.Failure(new Error("stock.currency_conflict",
                        $"This space uses {existingPurchases.First().Cost.Currency}. Cannot add purchase in {command.CostCurrency}."));
                }
            }

            coffeeStock.AddPurchase(
                product,
                quantity,
                cost,
                command.Vendor,
                currentUserId,
                command.PurchasedAt,
                _clock);

            if (!isNewStock)
            {
                await _coffeeStockRepository.UpdateAsync(coffeeStock, cancellationToken);
            }

            var latestPurchase = coffeeStock.Purchases.OrderByDescending(p => p.CreatedAt).First();

            var dto = new StockPurchaseDto
            {
                Id = latestPurchase.Id,
                ProductName = latestPurchase.Product.Name,
                ProductBrand = latestPurchase.Product.Brand,
                ProductType = latestPurchase.Product.Type.ToString(),
                QuantityGrams = latestPurchase.Quantity.Grams,
                CostAmount = latestPurchase.Cost.Amount,
                CostCurrency = latestPurchase.Cost.Currency,
                Vendor = latestPurchase.Vendor,
                PurchasedBy = latestPurchase.PurchasedBy.Value,
                PurchasedAt = latestPurchase.PurchasedAt,
                CreatedAt = latestPurchase.CreatedAt,
                CostPerGram = latestPurchase.CostPerGram.Amount
            };

            return Result<StockPurchaseDto>.Success(dto);
        }
        catch (ArgumentException ex)
        {
            return Result<StockPurchaseDto>.Failure(Error.ValidationFailure("Purchase", ex.Message));
        }
        catch (Exception ex)
        {
            // TODO: Remove this diagnostic - expose actual error for debugging
            Console.WriteLine($"=== EXCEPTION IN AddStockPurchaseCommandHandler ===");
            Console.WriteLine($"Type: {ex.GetType().FullName}");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            return Result<StockPurchaseDto>.Failure(new Error("SYSTEM_ERROR", ex.Message));
        }
    }
}