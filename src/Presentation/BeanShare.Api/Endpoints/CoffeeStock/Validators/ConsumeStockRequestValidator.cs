using BeanShare.Contracts.CoffeeStock;
using FastEndpoints;
using FluentValidation;

namespace BeanShare.Api.Endpoints.CoffeeStock.Validators;

public sealed class ConsumeStockRequestValidator : Validator<ConsumeStockRequest>
{
    public ConsumeStockRequestValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage("SpaceId is required");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(200)
            .WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.ProductBrand)
            .NotEmpty()
            .WithMessage("Product brand is required")
            .MaximumLength(200)
            .WithMessage("Product brand cannot exceed 200 characters");

        RuleFor(x => x.ProductType)
            .NotEmpty()
            .WithMessage("Product type is required");

        RuleFor(x => x.QuantityGrams)
            .GreaterThan(0)
            .WithMessage("Quantity must be positive");
    }
}