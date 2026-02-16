using BeanShare.Application.Features.CoffeeStock.Queries;
using BeanShare.Contracts.CoffeeStock;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.CoffeeStock;

public sealed class GetSpaceLowStockEndpoint : Endpoint<GetLowStockAlertsRequest, LowStockAlertsResponse>
{
    private readonly IMediator _mediator;

    public GetSpaceLowStockEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/coffeestock/{spaceId}/low-stock");
        Summary(s =>
        {
            s.Summary = "Get low stock products for space";
            s.Description = "Retrieves products that are running low on stock for the space using the new API pattern. Requires space membership.";
            s.ExampleRequest = new GetLowStockAlertsRequest
            {
                SpaceId = Guid.NewGuid(),
                ThresholdGrams = 100
            };
        });
    }

    public override async Task HandleAsync(GetLowStockAlertsRequest req, CancellationToken ct)
    {
        var routeSpaceId = Route<Guid>("spaceId");
        var thresholdGrams = Query<decimal?>("threshold", false) ?? req.ThresholdGrams;

        var query = new GetLowStockAlertsQuery(routeSpaceId, thresholdGrams);
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Code, error.Message);
            }
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var alerts = result.Value.Select(alert => new LowStockAlertResponse
        {
            StockLevelId = alert.StockLevelId,
            ProductName = alert.ProductName,
            ProductBrand = alert.ProductBrand,
            ProductType = alert.ProductType,
            ProductDisplayName = alert.ProductDisplayName,
            CurrentStockGrams = alert.CurrentStockGrams,
            ThresholdGrams = alert.ThresholdGrams,
            AlertLevel = alert.AlertLevel,
            LastUpdated = alert.LastUpdated
        }).ToList();

        var response = new LowStockAlertsResponse
        {
            SpaceId = routeSpaceId,
            AlertCount = alerts.Count,
            Alerts = alerts
        };

        await SendOkAsync(response, ct);
    }
}