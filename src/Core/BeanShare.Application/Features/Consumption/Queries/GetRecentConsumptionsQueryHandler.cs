using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Consumption.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Consumption.Queries;

public sealed class GetRecentConsumptionsQueryHandler
    : IRequestHandler<GetRecentConsumptionsQuery, Result<GetRecentConsumptionsResult>>
{
    private readonly IConsumptionRepository _consumptionRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public GetRecentConsumptionsQueryHandler(
        IConsumptionRepository consumptionRepository,
        IUserContext userContext,
        IClock clock)
    {
        _consumptionRepository = consumptionRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<GetRecentConsumptionsResult>> Handle(
        GetRecentConsumptionsQuery request,
        CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var userId = _userContext.CurrentUserId;

        var allEntries = await _consumptionRepository.GetBySpaceIdAsync(spaceId, cancellationToken);
        var recentEntries = allEntries
            .OrderByDescending(e => e.ConsumedAt)
            .Take(20)
            .Select(e => new ConsumptionEntryDto
            {
                Id = e.Id.Value,
                SpaceId = e.SpaceId.Value,
                UserId = e.UserId.Value,
                ProductName = e.Product?.Name ?? "Unknown",
                ProductBrand = e.Product?.Brand ?? "Unknown",
                ProductType = e.Product?.Type.ToString() ?? "Unknown",
                QuantityGrams = e.Quantity.Grams,
                RemainingGrams = 0,
                ConsumedAt = e.ConsumedAt,
                CreatedAt = e.CreatedAt,
                PresetId = e.PresetId?.Value,
                PresetName = e.PresetName
            })
            .ToList();

        var today = _clock.UtcNow.Date;
        var myEntriesToday = allEntries
            .Where(e => e.UserId == userId && e.ConsumedAt.Date == today)
            .ToList();

        var myCupsToday = myEntriesToday.Count;
        var myTotalCost = 0m;

        var remainingStock = 500;

        var result = new GetRecentConsumptionsResult(
            recentEntries,
            myCupsToday,
            myTotalCost,
            remainingStock
        );

        return Result<GetRecentConsumptionsResult>.Success(result);
    }
}
