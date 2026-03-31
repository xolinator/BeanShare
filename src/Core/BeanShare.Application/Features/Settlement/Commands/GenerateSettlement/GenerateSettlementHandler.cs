using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Application.Services;
using BeanShare.Domain.Aggregates.Settlement;
using BeanShare.Domain.Common;
using BeanShare.Domain.Enums;
using BeanShare.Domain.Services;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Settlement.Commands.GenerateSettlement;
public sealed class GenerateSettlementHandler : IRequestHandler<GenerateSettlementCommand, Result<SettlementDto>>
{
    private readonly ISettlementRepository _settlementRepository;
    private readonly IBillingPeriodRepository _billingPeriodRepository;
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IUserService _userService;
    private readonly IClock _clock;

    public GenerateSettlementHandler(
        ISettlementRepository settlementRepository,
        IBillingPeriodRepository billingPeriodRepository,
        IConsumptionRepository consumptionRepository,
        ICoffeeStockRepository coffeeStockRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IUserService userService,
        IClock clock)
    {
        _settlementRepository = settlementRepository;
        _billingPeriodRepository = billingPeriodRepository;
        _consumptionRepository = consumptionRepository;
        _coffeeStockRepository = coffeeStockRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _userService = userService;
        _clock = clock;
    }

    public async Task<Result<SettlementDto>> Handle(GenerateSettlementCommand command, CancellationToken cancellationToken)
    {
        var currentUserId = _userContext.CurrentUserId;
        var billingPeriodId = new BillingPeriodId(command.BillingPeriodId);

        var billingPeriod = await _billingPeriodRepository.GetByIdAsync(billingPeriodId, cancellationToken);
        if (billingPeriod == null)
        {
            return Result<SettlementDto>.Failure(Error.BillingPeriodNotFound(command.BillingPeriodId));
        }

        if (billingPeriod.State != BillingState.Closed)
        {
            return Result<SettlementDto>.Failure(
                Error.InvalidBillingPeriodState("generate settlement", billingPeriod.State.ToString()));
        }

        var existingSettlement = await _settlementRepository.GetByBillingPeriodIdAsync(billingPeriodId, cancellationToken);
        if (existingSettlement != null)
        {
            return Result<SettlementDto>.Failure(Error.SettlementAlreadyExists(command.BillingPeriodId));
        }

        var spaceSpec = new SpaceByIdSpecification(billingPeriod.SpaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);
        if (space == null)
        {
            return Result<SettlementDto>.Failure(Error.SpaceNotFound(billingPeriod.SpaceId.Value));
        }

        if (!space.IsAdmin(currentUserId))
        {
            return Result<SettlementDto>.Failure(Error.InsufficientSpacePrivileges("generate settlements"));
        }

        var consumptionSpec = new ConsumptionsByBillingPeriodSpecification(billingPeriod.SpaceId, billingPeriod.StartDate, billingPeriod.EndDate);
        var periodConsumptions = await _consumptionRepository.GetBySpecAsync(consumptionSpec, cancellationToken);

        if (!periodConsumptions.Any())
        {
            return Result<SettlementDto>.Failure(Error.NoConsumptionsInPeriod());
        }

        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(billingPeriod.SpaceId, cancellationToken);
        if (coffeeStock == null)
        {
            return Result<SettlementDto>.Failure(Error.SystemFailure("retrieving coffee stock"));
        }

        var periodPurchases = coffeeStock.Purchases
            .Where(p => p.PurchasedAt >= billingPeriod.StartDate && p.PurchasedAt <= billingPeriod.EndDate)
            .ToList();

        if (!periodPurchases.Any())
        {
            // Fall back to purchases up to and including the billing period end date
            // rather than ALL historical purchases which would distort cost calculations
            periodPurchases = coffeeStock.Purchases
                .Where(p => p.PurchasedAt <= billingPeriod.EndDate)
                .ToList();
        }

        var costingPolicy = new WeightedAverageCostingPolicy();
        var totalConsumedWeight = Weight.FromGrams(periodConsumptions.Sum(c => c.Quantity.Grams));
        var totalCost = costingPolicy.CalculateCost(periodPurchases, totalConsumedWeight, space.Currency.Code);

        var settlement = Domain.Aggregates.Settlement.Settlement.Create(
            billingPeriod.SpaceId,
            billingPeriodId,
            totalCost.Currency,
            currentUserId,
            _clock);

        if (totalConsumedWeight.Grams == 0)
        {
            return Result<SettlementDto>.Failure(Error.NoConsumptionsInPeriod());
        }

        var purchasesByUser = periodPurchases
            .Where(p => p.Cost.Currency == space.Currency.Code)
            .GroupBy(p => p.PurchasedBy)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Cost.Amount));

        var userConsumptions = periodConsumptions.GroupBy(c => c.UserId);

        foreach (var userGroup in userConsumptions)
        {
            var userId = userGroup.Key;
            var userTotalCoffee = userGroup.Sum(c => c.Quantity.Grams);
            var userPercentage = userTotalCoffee / totalConsumedWeight.Grams;
            var userAmount = totalCost.Amount * userPercentage;

            settlement.AddLine(
                userId,
                userTotalCoffee,
                userAmount,
                _clock,
                null
            );
        }

        settlement.FinalizeGeneration(_clock);

        await _settlementRepository.AddAsync(settlement, cancellationToken);

        billingPeriod.MarkAsSettled(currentUserId, _clock);
        await _billingPeriodRepository.UpdateAsync(billingPeriod, cancellationToken);

        var userIds = settlement.Lines.Select(l => l.UserId).Distinct().ToList();
        var users = await _userService.GetByIdsAsync(userIds, cancellationToken);
        var userLookup = users.ToDictionary(u => u.Id);

        var lines = new List<SettlementLineDto>();
        foreach (var line in settlement.Lines)
        {
            userLookup.TryGetValue(line.UserId, out var user);
            var userName = user?.Name ?? $"User {line.UserId.Value}";
            var userEmail = user?.Email ?? $"user{line.UserId.Value}@example.com";
            var amountPaid = purchasesByUser.TryGetValue(line.UserId, out var paid) ? paid : 0m;
            var netBalance = amountPaid - line.AmountDue.Amount;

            lines.Add(new SettlementLineDto(
                line.UserId.Value,
                userName,
                userEmail,
                line.TotalCoffeeGrams,
                line.TotalMilkMl,
                line.AmountDue.Amount,
                amountPaid,
                netBalance,
                line.AmountDue.Currency,
                line.GetConsumptionPercentage(totalConsumedWeight.Grams),
                line.IsConfirmed,
                line.Confirmation?.ConfirmedBy.Value,
                line.Confirmation?.ConfirmedAt
            ));
        }

        var dto = new SettlementDto(
            settlement.Id.Value,
            settlement.SpaceId.Value,
            settlement.BillingPeriodId.Value,
            billingPeriod.Name,
            billingPeriod.StartDate,
            billingPeriod.EndDate,
            settlement.GeneratedAt,
            settlement.GeneratedBy.Value,
            settlement.TotalAmount,
            settlement.Currency,
            settlement.Status.ToString(),
            settlement.CompletedAt,
            settlement.ConfirmedLinesCount,
            settlement.TotalLinesCount,
            lines
        );

        return Result<SettlementDto>.Success(dto);
    }
}