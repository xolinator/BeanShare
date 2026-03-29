using BeanShare.Application.Features.Consumption.Queries;
using BeanShare.Application.Features.CoffeeStock.Queries;
using BeanShare.Application.Features.Presets.Queries;
using BeanShare.Application.Features.Spaces.Queries;
using BeanShare.Domain.ValueObjects;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Spaces;

public sealed class GetSpaceDashboardRequest
{
    public Guid SpaceId { get; set; }
}

/// <summary>
/// Composite endpoint that returns space info, stock, recent consumption, and quick presets
/// in a single round-trip. Designed for mobile clients where network latency matters.
/// </summary>
public sealed class GetSpaceDashboardEndpoint(IMediator mediator)
    : Endpoint<GetSpaceDashboardRequest>
{
    public override void Configure()
    {
        Get("/api/spaces/{SpaceId}/dashboard");
        Summary(s =>
        {
            s.Summary = "Get space dashboard data";
            s.Description = "Returns space details, stock levels, recent consumption, and quick presets in a single call. Optimized for mobile clients.";
        });
    }

    public override async Task HandleAsync(GetSpaceDashboardRequest req, CancellationToken ct)
    {
        var spaceTask = mediator.Send(new GetSpaceByIdQuery(new SpaceId(req.SpaceId)), ct);
        var stockTask = mediator.Send(new GetSpaceStockQuery(req.SpaceId), ct);
        var consumptionTask = mediator.Send(new GetRecentConsumptionsQuery(req.SpaceId, null), ct);
        var presetsTask = mediator.Send(new GetQuickPresetsQuery(req.SpaceId), ct);

        await Task.WhenAll(spaceTask, stockTask, consumptionTask, presetsTask);

        var spaceResult = await spaceTask;
        if (!spaceResult.IsSuccess)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var space = spaceResult.Value;
        var stock = (await stockTask).IsSuccess ? (await stockTask).Value : null;
        var consumption = (await consumptionTask).IsSuccess ? (await consumptionTask).Value : null;
        var presets = (await presetsTask).IsSuccess ? (await presetsTask).Value : null;

        var response = new
        {
            Space = new
            {
                space.Id,
                space.Name,
                space.InviteCode,
                space.CurrencyCode,
                space.IsActive,
                space.MemberCount,
                space.CreatedAt,
                Members = space.Members
            },
            Stock = stock != null ? new
            {
                stock.TotalCurrentStockGrams,
                stock.TotalInvestmentAmount,
                stock.TotalInvestmentCurrency,
                StockLevels = stock.StockLevels.Select(sl => new
                {
                    sl.Id, sl.ProductName, sl.ProductBrand, sl.ProductType,
                    sl.ProductDisplayName, sl.CurrentStockGrams, sl.TotalPurchasedGrams,
                    sl.TotalConsumedGrams, sl.ConsumptionPercentage, sl.IsArchived
                })
            } : null,
            Consumption = consumption,
            Presets = presets
        };

        await SendOkAsync(response, ct);
    }
}
