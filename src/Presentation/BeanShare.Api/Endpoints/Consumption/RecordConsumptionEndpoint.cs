using BeanShare.Api.Endpoints.Consumption.Validators;
using BeanShare.Application.Features.Consumption.Commands;
using BeanShare.Contracts.Consumption;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Consumption;

public sealed class RecordConsumptionEndpoint(IMediator mediator)
    : Endpoint<RecordConsumptionRequest, RecordConsumptionResponse>
{
    public override void Configure()
    {
        Post("/api/consumptions");
        AllowAnonymous(); // TODO: Add authentication when OIDC is configured
        Validator<RecordConsumptionRequestValidator>();
        Summary(s =>
        {
            s.Summary = "Record coffee consumption";
            s.Description = "Records coffee consumption from stock. Deducts from inventory and creates consumption entry.";
            s.ExampleRequest = new RecordConsumptionRequest
            {
                SpaceId = Guid.NewGuid(),
                ProductName = "Premium Espresso Blend",
                ProductBrand = "Blue Mountain Coffee",
                ProductType = "Espresso",
                QuantityGrams = 15.0m,
                ConsumedAt = DateTime.UtcNow.AddMinutes(-30)
            };
        });
    }

    public override async Task HandleAsync(RecordConsumptionRequest req, CancellationToken ct)
    {
        var command = new RecordConsumptionCommand(
            req.SpaceId,
            req.ProductName,
            req.ProductBrand,
            req.ProductType,
            req.QuantityGrams,
            req.ConsumedAt);

        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Errors.Any(e => e.Code.StartsWith("stock.not_found")))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (result.Errors.Any(e => e.Code.StartsWith("stock.product_not_found")))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (result.Errors.Any(e => e.Code.StartsWith("stock.insufficient")))
            {
                await SendAsync(new RecordConsumptionResponse
                {
                    SpaceId = req.SpaceId,
                    ProductName = req.ProductName,
                    ProductBrand = req.ProductBrand,
                    ProductType = req.ProductType,
                    ConsumedGrams = 0,
                    RemainingGrams = 0,
                    ConsumedAt = req.ConsumedAt ?? DateTime.UtcNow
                }, 409, ct);
                return;
            }

            foreach (var error in result.Errors)
            {
                AddError(error.Code, error.Message);
            }
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var consumption = result.Value;
        var response = new RecordConsumptionResponse
        {
            SpaceId = consumption.SpaceId,
            ProductName = consumption.ProductName,
            ProductBrand = consumption.ProductBrand,
            ProductType = consumption.ProductType,
            ConsumedGrams = consumption.QuantityGrams,
            RemainingGrams = consumption.RemainingGrams,
            ConsumedAt = consumption.ConsumedAt
        };

        await SendAsync(response, 201, cancellation: ct);
    }
}