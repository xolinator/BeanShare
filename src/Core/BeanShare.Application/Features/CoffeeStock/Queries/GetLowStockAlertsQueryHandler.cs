using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.CoffeeStock.Dtos;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.CoffeeStock.Queries;

public sealed class GetLowStockAlertsQueryHandler : IRequestHandler<GetLowStockAlertsQuery, Result<IEnumerable<LowStockAlertDto>>>
{
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;

    public GetLowStockAlertsQueryHandler(
        ICoffeeStockRepository coffeeStockRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext)
    {
        _coffeeStockRepository = coffeeStockRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
    }

    public async Task<Result<IEnumerable<LowStockAlertDto>>> Handle(GetLowStockAlertsQuery query, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(query.SpaceId);
        var spaceSpec = new SpaceByIdSpecification(spaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);

        if (space == null)
        {
            return Result<IEnumerable<LowStockAlertDto>>.Failure(Error.SpaceNotFound(query.SpaceId));
        }

        var currentUserId = _userContext.CurrentUserId;
        if (!space.HasMember(currentUserId))
        {
            return Result<IEnumerable<LowStockAlertDto>>.Failure(Error.InsufficientSpacePrivileges("view stock alerts"));
        }

        var threshold = Weight.FromGrams(query.ThresholdGrams);
        var lowStockSpec = new LowStockSpecification(spaceId, threshold);
        var coffeeStock = await _coffeeStockRepository.GetSingleBySpecAsync(lowStockSpec, cancellationToken);

        if (coffeeStock == null)
        {
            return Result<IEnumerable<LowStockAlertDto>>.Success([]);
        }

        var lowStockProducts = coffeeStock.GetLowStockProducts(threshold);

        var alerts = lowStockProducts.Select(stockLevel => new LowStockAlertDto
        {
            StockLevelId = stockLevel.Id,
            ProductName = stockLevel.Product.Name,
            ProductBrand = stockLevel.Product.Brand,
            ProductType = stockLevel.Product.Type.ToString(),
            ProductDisplayName = stockLevel.Product.DisplayName,
            CurrentStockGrams = stockLevel.CurrentStock.Grams,
            ThresholdGrams = threshold.Grams,
            AlertLevel = GetAlertLevel(stockLevel.CurrentStock, threshold),
            LastUpdated = stockLevel.UpdatedAt
        });

        return Result<IEnumerable<LowStockAlertDto>>.Success(alerts);
    }

    private static string GetAlertLevel(Weight currentStock, Weight threshold)
    {
        if (currentStock.IsZero)
        {
            return "Critical";
        }

        if (currentStock <= threshold.Multiply(0.5m))
        {
            return "High";
        }

        if (currentStock <= threshold)
        {
            return "Medium";
        }

        return "Low";
    }
}