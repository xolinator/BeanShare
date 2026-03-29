using BeanShare.Application.Features.Consumption.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using FluentValidation;

namespace BeanShare.Application.Features.Consumption.Commands;

public sealed class ImportConsumptionCsvValidator : AbstractValidator<ImportConsumptionCsvCommand>
{
    private const int MaxRows = 1000;
    private readonly IClock _clock;

    public ImportConsumptionCsvValidator(IClock clock)
    {
        _clock = clock;
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage("SpaceId is required");

        RuleFor(x => x.Rows)
            .NotEmpty()
            .WithMessage("CSV file contains no data rows")
            .Must(rows => rows.Count <= MaxRows)
            .WithMessage($"CSV file cannot contain more than {MaxRows} rows");

        RuleForEach(x => x.Rows).SetValidator(new ImportRowValidator(_clock));
    }

    private sealed class ImportRowValidator : AbstractValidator<ImportConsumptionCsvRow>
    {
        private readonly IClock _clock;

        public ImportRowValidator(IClock clock)
        {
            _clock = clock;
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("ProductName is required")
                .MaximumLength(200);

            RuleFor(x => x.ProductBrand)
                .NotEmpty().WithMessage("ProductBrand is required")
                .MaximumLength(200);

            RuleFor(x => x.ProductType)
                .NotEmpty().WithMessage("ProductType is required")
                .Must(pt => Enum.TryParse<CoffeeType>(pt, true, out _))
                .WithMessage("Must be one of: Espresso, Filter, Instant, Decaf, Specialty");

            RuleFor(x => x.QuantityGrams)
                .GreaterThan(0).WithMessage("Must be greater than 0");

            RuleFor(x => x.ConsumedAt)
                .Must(date => date <= _clock.UtcNow.AddMinutes(5))
                .WithMessage("Date cannot be in the future");
        }
    }
}
