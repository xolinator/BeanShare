using BeanShare.Contracts.Consumption;
using FastEndpoints;
using FluentValidation;

namespace BeanShare.Api.Endpoints.Consumption.Validators;

public sealed class RecordConsumptionRequestValidator : Validator<RecordConsumptionRequest>
{
    public RecordConsumptionRequestValidator()
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