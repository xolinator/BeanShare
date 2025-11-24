using FluentValidation;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Consumption.Commands;

public sealed class RecordConsumptionValidator : AbstractValidator<RecordConsumptionCommand>
{
    public RecordConsumptionValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage($"{nameof(RecordConsumptionCommand.SpaceId)} is required");

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .WithMessage($"{nameof(RecordConsumptionCommand.ProductName)} is required")
            .MaximumLength(200)
            .WithMessage($"{nameof(RecordConsumptionCommand.ProductName)} cannot exceed 200 characters");

        RuleFor(x => x.ProductBrand)
            .NotEmpty()
            .WithMessage($"{nameof(RecordConsumptionCommand.ProductBrand)} is required")
            .MaximumLength(200)
            .WithMessage($"{nameof(RecordConsumptionCommand.ProductBrand)} cannot exceed 200 characters");

        RuleFor(x => x.ProductType)
            .NotEmpty()
            .WithMessage($"{nameof(RecordConsumptionCommand.ProductType)} is required")
            .Must(pt => Enum.TryParse<CoffeeType>(pt, true, out _))
            .WithMessage($"{nameof(RecordConsumptionCommand.ProductType)} must be one of: Espresso, Filter, Instant, Decaf, Specialty");

        RuleFor(x => x.QuantityGrams)
            .GreaterThan(0)
            .WithMessage($"{nameof(RecordConsumptionCommand.QuantityGrams)} must be positive");

        RuleFor(x => x.ConsumedAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.ConsumedAt.HasValue)
            .WithMessage($"{nameof(RecordConsumptionCommand.ConsumedAt)} cannot be in the future");
    }
}