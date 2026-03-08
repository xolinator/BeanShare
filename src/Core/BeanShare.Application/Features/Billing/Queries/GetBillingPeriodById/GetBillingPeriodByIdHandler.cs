using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Billing.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.Services;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Billing.Queries.GetBillingPeriodById;

public sealed class GetBillingPeriodByIdHandler : IRequestHandler<GetBillingPeriodByIdQuery, Result<BillingPeriodDto>>
{
    private readonly IBillingPeriodRepository _billingPeriodRepository;
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly ICostingPolicy _costingPolicy;

    public GetBillingPeriodByIdHandler(
        IBillingPeriodRepository billingPeriodRepository,
        IConsumptionRepository consumptionRepository,
        ICoffeeStockRepository coffeeStockRepository,
        ISpaceRepository spaceRepository,
        ICostingPolicy costingPolicy)
    {
        _billingPeriodRepository = billingPeriodRepository;
        _consumptionRepository = consumptionRepository;
        _coffeeStockRepository = coffeeStockRepository;
        _spaceRepository = spaceRepository;
        _costingPolicy = costingPolicy;
    }

    public async Task<Result<BillingPeriodDto>> Handle(GetBillingPeriodByIdQuery request, CancellationToken cancellationToken)
    {
        var billingPeriod = await _billingPeriodRepository.GetByIdAsync(request.Id, cancellationToken);

        if (billingPeriod is null)
        {
            return Result<BillingPeriodDto>.Failure(Error.BillingPeriodNotFound(request.Id.Value));
        }

        var consumptions = await _consumptionRepository.GetBySpaceIdAsync(billingPeriod.SpaceId, cancellationToken);

        var periodConsumptions = consumptions
            .Where(c => c.ConsumedAt >= billingPeriod.StartDate && c.ConsumedAt <= billingPeriod.EndDate)
            .ToList();

        var consumptionCount = periodConsumptions.Count;
        var totalGrams = periodConsumptions.Sum(c => c.Quantity.Grams);

        var space = await _spaceRepository.GetByIdAsync(billingPeriod.SpaceId, cancellationToken);
        if (space is null)
        {
            return Result<BillingPeriodDto>.Failure(Error.SpaceNotFound(billingPeriod.SpaceId.Value));
        }

        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(billingPeriod.SpaceId, cancellationToken);

        decimal? estimatedCost = null;
        string currency = space.Currency.Code;

        if (totalGrams > 0 && coffeeStock is not null)
        {
            var totalConsumed = Weight.FromGrams(totalGrams);
            var cost = _costingPolicy.CalculateCost(coffeeStock.Purchases, totalConsumed, currency);
            estimatedCost = cost.Amount;
        }

        var dto = new BillingPeriodDto(
            billingPeriod.Id.Value,
            billingPeriod.SpaceId.Value,
            billingPeriod.Name,
            billingPeriod.StartDate,
            billingPeriod.EndDate,
            billingPeriod.State.ToString(),
            billingPeriod.CreatedAt,
            billingPeriod.CreatedBy.Value,
            billingPeriod.State == BillingState.Open ? billingPeriod.CreatedAt : (DateTime?)null, // OpenedAt
            billingPeriod.State == BillingState.Open ? billingPeriod.CreatedBy.Value : (Guid?)null, // OpenedBy
            billingPeriod.ClosedAt,
            billingPeriod.ClosedBy?.Value,
            billingPeriod.SettledAt,
            billingPeriod.SettledBy?.Value,
            consumptionCount,
            totalGrams,
            estimatedCost,
            currency
        );

        return Result<BillingPeriodDto>.Success(dto);
    }
}