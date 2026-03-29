using BeanShare.Domain.ValueObjects;
using FluentValidation;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed class ConsumeStockValidator : AbstractValidator<ConsumeStockCommand>
{
    public ConsumeStockValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage($"{nameof(ConsumeStockCommand.SpaceId)} is required");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage($"{nameof(ConsumeStockCommand.ProductName)} is required")
            .MaximumLength(200)
            .WithMessage($"{nameof(ConsumeStockCommand.ProductName)} cannot exceed 200 characters");

        RuleFor(x => x.ProductBrand)
            .NotEmpty()
            .WithMessage($"{nameof(ConsumeStockCommand.ProductBrand)} is required")
            .MaximumLength(200)
            .WithMessage($"{nameof(ConsumeStockCommand.ProductBrand)} cannot exceed 200 characters");

        RuleFor(x => x.ProductType)
            .NotEmpty().WithMessage($"{nameof(ConsumeStockCommand.ProductType)} is required")
            .Must(pt => Enum.TryParse<CoffeeType>(pt, true, out _))
            .WithMessage($"{nameof(ConsumeStockCommand.ProductType)} must be one of: Espresso, Filter, Instant, Decaf, Specialty");

        RuleFor(x => x.QuantityGrams)
            .GreaterThan(0)
            .WithMessage($"{nameof(ConsumeStockCommand.QuantityGrams)} must be positive");
    }
}
