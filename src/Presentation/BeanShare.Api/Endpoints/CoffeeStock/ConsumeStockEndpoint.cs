using BeanShare.Api.Endpoints.CoffeeStock.Validators;
using BeanShare.Application.Features.CoffeeStock.Commands;
using BeanShare.Contracts.CoffeeStock;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.CoffeeStock;

public sealed class ConsumeStockEndpoint : Endpoint<ConsumeStockRequest, ConsumeStockResponse>
{
    private readonly IMediator _mediator;

    public ConsumeStockEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/coffeestock/consume");
        AllowAnonymous(); // TODO: Add authentication when OIDC is configured
        Validator<ConsumeStockRequestValidator>();
        Summary(s =>
        {
            s.Summary = "Consume coffee stock";
            s.Description = "Records coffee consumption from stock. Requires admin privileges. Updates stock levels and tracks consumption.";
            s.ExampleRequest = new ConsumeStockRequest
            {
                SpaceId = Guid.NewGuid(),
                ProductName = "Premium Espresso Blend",
                ProductBrand = "Blue Mountain Coffee",
                ProductType = "Espresso",
                QuantityGrams = 15
            };
        });
    }

    public override async Task HandleAsync(ConsumeStockRequest req, CancellationToken ct)
    {
        var command = new ConsumeStockCommand(
            req.SpaceId,
            req.ProductName,
            req.ProductBrand,
            req.ProductType,
            req.QuantityGrams);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            if (result.Errors.Any(e => e.Code == "stock.product_not_found"))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            if (result.Errors.Any(e => e.Code == "stock.insufficient"))
            {
                await SendAsync(new ConsumeStockResponse
                {
                    SpaceId = req.SpaceId,
                    ProductName = req.ProductName,
                    ProductBrand = req.ProductBrand,
                    ProductType = req.ProductType,
                    ConsumedGrams = 0,
                    RemainingGrams = 0,
                    Message = result.Errors.First(e => e.Code == "stock.insufficient").Message
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

        var consumedStock = result.Value;
        var response = new ConsumeStockResponse
        {
            SpaceId = consumedStock.SpaceId,
            ProductName = consumedStock.ProductName,
            ProductBrand = consumedStock.ProductBrand,
            ProductType = consumedStock.ProductType,
            ConsumedGrams = consumedStock.ConsumedGrams,
            RemainingGrams = consumedStock.RemainingGrams,
            Message = $"Successfully consumed {consumedStock.ConsumedGrams}g of {consumedStock.ProductBrand} {consumedStock.ProductName}"
        };

        await SendOkAsync(response, ct);
    }
}