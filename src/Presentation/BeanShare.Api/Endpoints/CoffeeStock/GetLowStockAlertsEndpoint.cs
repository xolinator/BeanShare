using BeanShare.Api.Endpoints.Common;
using BeanShare.Application.Features.CoffeeStock.Queries;
using BeanShare.Contracts.CoffeeStock;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.CoffeeStock;

public sealed class GetLowStockAlertsEndpoint : Endpoint<GetLowStockAlertsRequest, LowStockAlertsResponse>
{
    private readonly IMediator _mediator;

    public GetLowStockAlertsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/spaces/{spaceId}/stock/alerts");
        Summary(s =>
        {
            s.Summary = "Get low stock alerts";
            s.Description = "Retrieves products that are running low on stock for the space. Requires space membership. Default threshold is 100 grams.";
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

        req = req with
        {
            SpaceId = routeSpaceId,
            ThresholdGrams = thresholdGrams
        };

        var query = new GetLowStockAlertsQuery(req.SpaceId, req.ThresholdGrams);
        var result = await _mediator.Send(query, ct);

        if (result.IsSuccess)
        {
            var alertDtos = result.Value;
            var alerts = alertDtos.Select(alert => new LowStockAlertResponse
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
                SpaceId = req.SpaceId,
                AlertCount = alerts.Count,
                Alerts = alerts
            };
            await SendOkAsync(response, ct);
        }
        else
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Code, error.Message);
            }
            await SendErrorsAsync(cancellation: ct);
        }
    }
}