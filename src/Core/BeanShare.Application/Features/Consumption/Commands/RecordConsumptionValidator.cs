using FluentValidation;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Consumption.Commands;

public sealed class RecordConsumptionValidator : AbstractValidator<RecordConsumptionCommand>
{
    public RecordConsumptionValidator()
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
            .WithMessage("Product type is required")
            .Must(pt => Enum.TryParse<CoffeeType>(pt, true, out _))
            .WithMessage("Product type must be one of: Espresso, Filter, Instant, Decaf, Specialty");

        RuleFor(x => x.QuantityGrams)
            .GreaterThan(0)
            .WithMessage("Quantity must be positive");

        RuleFor(x => x.ConsumedAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.ConsumedAt.HasValue)
            .WithMessage("Consumption time cannot be in the future");
    }
}