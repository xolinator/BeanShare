using FluentValidation;

namespace BeanShare.Application.Features.CoffeeStock.Commands;

public sealed class AddStockPurchaseValidator : AbstractValidator<AddStockPurchaseCommand>
{
    public AddStockPurchaseValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage($"{nameof(AddStockPurchaseCommand.SpaceId)} is required");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage($"{nameof(AddStockPurchaseCommand.ProductName)} is required")
            .MaximumLength(100)
            .WithMessage($"{nameof(AddStockPurchaseCommand.ProductName)} cannot exceed 100 characters");

        RuleFor(x => x.ProductBrand)
            .NotEmpty()
            .WithMessage($"{nameof(AddStockPurchaseCommand.ProductBrand)} is required")
            .MaximumLength(50)
            .WithMessage($"{nameof(AddStockPurchaseCommand.ProductBrand)} cannot exceed 50 characters");

        RuleFor(x => x.ProductType)
            .NotEmpty()
            .WithMessage($"{nameof(AddStockPurchaseCommand.ProductType)} is required")
            .Must(BeValidCoffeeType)
            .WithMessage($"{nameof(AddStockPurchaseCommand.ProductType)} must be one of: Espresso, Filter, Instant, Decaf, Specialty");

        RuleFor(x => x.QuantityGrams)
            .GreaterThan(0)
            .WithMessage($"{nameof(AddStockPurchaseCommand.QuantityGrams)} must be positive")
            .LessThanOrEqualTo(50000)
            .WithMessage($"{nameof(AddStockPurchaseCommand.QuantityGrams)} cannot exceed 50kg");

        RuleFor(x => x.CostAmount)
            .GreaterThan(0)
            .WithMessage($"{nameof(AddStockPurchaseCommand.CostAmount)} must be positive")
            .LessThanOrEqualTo(10000)
            .WithMessage($"{nameof(AddStockPurchaseCommand.CostAmount)} cannot exceed 10,000");

        RuleFor(x => x.CostCurrency)
            .NotEmpty()
            .WithMessage($"{nameof(AddStockPurchaseCommand.CostCurrency)} is required")
            .Length(3)
            .WithMessage($"{nameof(AddStockPurchaseCommand.CostCurrency)} must be 3 characters");

        RuleFor(x => x.Vendor)
            .NotEmpty()
            .WithMessage($"{nameof(AddStockPurchaseCommand.Vendor)} is required")
            .MaximumLength(100)
            .WithMessage($"{nameof(AddStockPurchaseCommand.Vendor)} cannot exceed 100 characters");

        RuleFor(x => x.PurchasedAt)
            .NotEmpty()
            .WithMessage($"{nameof(AddStockPurchaseCommand.PurchasedAt)} is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage($"{nameof(AddStockPurchaseCommand.PurchasedAt)} cannot be in the future");
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
