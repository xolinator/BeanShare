using BeanShare.Api.Endpoints.CoffeeStock.Validators;
using BeanShare.Application.Features.CoffeeStock.Commands;
using BeanShare.Contracts.CoffeeStock;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.CoffeeStock;

public sealed class AddStockPurchaseEndpoint : Endpoint<AddStockPurchaseRequest, StockPurchaseResponse>
{
    private readonly IMediator _mediator;

    public AddStockPurchaseEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/spaces/{spaceId}/stock/purchases");
        AllowAnonymous(); // TODO: Add authentication when OIDC is configured
        Validator<AddStockPurchaseRequestValidator>();
        Summary(s =>
        {
            s.Summary = "Add coffee stock purchase";
            s.Description = "Records a new coffee purchase for the space. Requires admin privileges. Updates stock levels and creates purchase history. Route parameters must match request body.";
            s.ExampleRequest = new AddStockPurchaseRequest
            {
                SpaceId = Guid.NewGuid(),
                ProductName = "Premium Espresso Blend",
                ProductBrand = "Blue Mountain Coffee",
                ProductType = "Espresso",
                QuantityGrams = 1000,
                CostAmount = 25.99m,
                CostCurrency = "USD",
                Vendor = "Local Coffee Roasters",
                PurchasedAt = DateTime.UtcNow.AddDays(-1)
            };
        });
    }

    public override async Task HandleAsync(AddStockPurchaseRequest req, CancellationToken ct)
    {
        var routeSpaceId = Route<Guid>("spaceId");

        if (req.SpaceId != routeSpaceId)
        {
            AddError("RouteParameterMismatch", "Route SpaceId must match request SpaceId");
        }

        if (ValidationFailed)
        {
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var command = new AddStockPurchaseCommand(
            req.SpaceId,
            req.ProductName,
            req.ProductBrand,
            req.ProductType,
            req.QuantityGrams,
            req.CostAmount,
            req.CostCurrency,
            req.Vendor,
            req.PurchasedAt);

        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Code, error.Message);
            }
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var dto = result.Value;
        var response = new StockPurchaseResponse
        {
            Id = dto.Id,
            ProductName = dto.ProductName,
            ProductBrand = dto.ProductBrand,
            ProductType = dto.ProductType,
            QuantityGrams = dto.QuantityGrams,
            CostAmount = dto.CostAmount,
            CostCurrency = dto.CostCurrency,
            Vendor = dto.Vendor,
            PurchasedBy = dto.PurchasedBy,
            PurchasedAt = dto.PurchasedAt,
            CreatedAt = dto.CreatedAt,
            CostPerGram = dto.CostPerGram,
            Message = $"Successfully added {dto.QuantityGrams}g of {dto.ProductBrand} {dto.ProductName}"
        };

        await SendCreatedAtAsync<GetSpaceStockEndpoint>(new { spaceId = req.SpaceId }, response, cancellation: ct);
    }
}