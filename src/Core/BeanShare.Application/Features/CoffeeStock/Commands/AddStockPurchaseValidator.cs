using FluentValidation;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed class AddStockPurchaseValidator : AbstractValidator<AddStockPurchaseCommand>
{
    public AddStockPurchaseValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage("Space ID is required");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(100)
            .WithMessage("Product name cannot exceed 100 characters");

        RuleFor(x => x.ProductBrand)
            .NotEmpty()
            .WithMessage("Product brand is required")
            .MaximumLength(50)
            .WithMessage("Product brand cannot exceed 50 characters");

        RuleFor(x => x.ProductType)
            .NotEmpty()
            .WithMessage("Product type is required")
            .Must(BeValidCoffeeType)
            .WithMessage("Product type must be one of: Espresso, Filter, Instant, Decaf, Specialty");

        RuleFor(x => x.QuantityGrams)
            .GreaterThan(0)
            .WithMessage("Quantity must be positive")
            .LessThanOrEqualTo(50000)
            .WithMessage("Quantity cannot exceed 50kg");

        RuleFor(x => x.CostAmount)
            .GreaterThan(0)
            .WithMessage("Cost must be positive")
            .LessThanOrEqualTo(10000)
            .WithMessage("Cost cannot exceed 10,000");

        RuleFor(x => x.CostCurrency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be 3 characters");

        RuleFor(x => x.Vendor)
            .NotEmpty()
            .WithMessage("Vendor is required")
            .MaximumLength(100)
            .WithMessage("Vendor cannot exceed 100 characters");

        RuleFor(x => x.PurchasedAt)
            .NotEmpty()
            .WithMessage("Purchase date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Purchase date cannot be in the future");
    }

    private static bool BeValidCoffeeType(string coffeeType)
    {
        return Enum.TryParse<CoffeeType>(coffeeType, out _);
    }
}

public enum CoffeeType
{
    Espresso = 1,
    Filter = 2,
    Instant = 3,
    Decaf = 4,
    Specialty = 5
}