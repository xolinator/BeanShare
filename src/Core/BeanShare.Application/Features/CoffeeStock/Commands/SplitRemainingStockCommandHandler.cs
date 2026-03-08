using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Services;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed class SplitRemainingStockCommandHandler : IRequestHandler<SplitRemainingStockCommand, Result<SplitRemainingStockResult>>
{
    private readonly ICoffeeStockRepository _coffeeStockRepository;
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserService _userService;
    private readonly IClock _clock;

    public SplitRemainingStockCommandHandler(
        ICoffeeStockRepository coffeeStockRepository,
        IConsumptionRepository consumptionRepository,
        ISpaceRepository spaceRepository,
        IUserService userService,
        IClock clock)
    {
        _coffeeStockRepository = coffeeStockRepository;
        _consumptionRepository = consumptionRepository;
        _spaceRepository = spaceRepository;
        _userService = userService;
        _clock = clock;
    }

    public async Task<Result<SplitRemainingStockResult>> Handle(SplitRemainingStockCommand request, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);

        var spaceSpec = new SpaceByIdSpecification(spaceId);
        var space = await _spaceRepository.GetSingleBySpecAsync(spaceSpec, cancellationToken);
        if (space == null)
        {
            return Result<SplitRemainingStockResult>.Failure(Error.SpaceNotFound(request.SpaceId));
        }

        var coffeeStock = await _coffeeStockRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        if (coffeeStock == null)
        {
            return Result<SplitRemainingStockResult>.Failure(Error.StockNotFound(request.SpaceId));
        }

        var stockLevel = coffeeStock.StockLevels.FirstOrDefault(sl => sl.Id == request.StockLevelId);
        if (stockLevel == null)
        {
            return Result<SplitRemainingStockResult>.Failure(Error.StockLevelNotFound(request.StockLevelId));
        }

        var remainingGrams = stockLevel.CurrentStock.Grams;
        if (remainingGrams <= 0)
        {
            return Result<SplitRemainingStockResult>.Failure(Error.NoRemainingStock());
        }

        var product = stockLevel.Product;

        var allConsumptions = await _consumptionRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        var productConsumptions = allConsumptions
            .Where(c => c.Product.Name == product.Name && c.Product.Brand == product.Brand && c.Product.Type == product.Type)
            .ToList();

        var memberIds = space.Members.Select(m => m.UserId).ToList();
        var users = await _userService.GetByIdsAsync(memberIds, cancellationToken);
        var userLookup = users.ToDictionary(u => u.Id, u => u.Name);

        var allocations = new List<MemberAllocationResult>();
        var consumptionByUser = productConsumptions
            .GroupBy(c => c.UserId)
            .ToDictionary(g => g.Key, g => g.Sum(c => c.Quantity.Grams));

        var totalConsumedByAllUsers = consumptionByUser.Values.Sum();

        if (totalConsumedByAllUsers > 0)
        {
            foreach (var (userId, userConsumedGrams) in consumptionByUser)
            {
                var percentage = (userConsumedGrams / totalConsumedByAllUsers) * 100;
                var allocatedGrams = Math.Round((userConsumedGrams / totalConsumedByAllUsers) * remainingGrams, 2);

                if (allocatedGrams > 0)
                {
                    allocations.Add(new MemberAllocationResult(
                        userId.Value,
                        userLookup.GetValueOrDefault(userId, "Unknown User"),
                        Math.Round(percentage, 2),
                        allocatedGrams));
                }
            }
        }
        else
        {
            var memberCount = memberIds.Count;
            var equalShare = Math.Round(remainingGrams / memberCount, 2);
            var equalPercentage = Math.Round(100.0m / memberCount, 2);

            foreach (var memberId in memberIds)
            {
                allocations.Add(new MemberAllocationResult(
                    memberId.Value,
                    userLookup.GetValueOrDefault(memberId, "Unknown User"),
                    equalPercentage,
                    equalShare));
            }
        }

        var now = _clock.UtcNow;
        foreach (var allocation in allocations.Where(a => a.AllocatedGrams > 0))
        {
            var quantity = Weight.FromGrams(allocation.AllocatedGrams);
            var consumptionEntry = ConsumptionEntry.Create(
                spaceId,
                new UserId(allocation.UserId),
                product,
                quantity,
                now,
                _clock,
                presetId: null,
                presetName: request.Reason);

            await _consumptionRepository.AddAsync(consumptionEntry, cancellationToken);
        }

        var totalDistributed = allocations.Sum(a => a.AllocatedGrams);
        coffeeStock.ConsumeStock(product, Weight.FromGrams(totalDistributed), _clock);
        await _coffeeStockRepository.UpdateAsync(coffeeStock, cancellationToken);

        return Result<SplitRemainingStockResult>.Success(new SplitRemainingStockResult(
            request.SpaceId,
            request.StockLevelId,
            product.Name,
            product.Brand,
            product.Type.ToString(),
            totalDistributed,
            allocations));
    }
}
