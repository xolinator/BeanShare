using BeanShare.Domain.Common;
using FluentValidation;

namespace BeanShare.Application.Features.Consumption.Commands;

public sealed class RecordConsumptionFromPresetValidator : AbstractValidator<RecordConsumptionFromPresetCommand>
{
    private const decimal MinCustomQuantityGrams = 0;
    private const decimal MaxCustomQuantityGrams = 100;
    private const int ClockSkewToleranceMinutes = 1;
    private readonly IClock _clock;

    public RecordConsumptionFromPresetValidator(IClock clock)
    {
        _clock = clock;
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage($"{nameof(RecordConsumptionFromPresetCommand.SpaceId)} is required");

        RuleFor(x => x.PresetId)
            .NotEmpty()
            .WithMessage($"{nameof(RecordConsumptionFromPresetCommand.PresetId)} is required");

        RuleFor(x => x.CustomQuantityGrams)
            .GreaterThan(MinCustomQuantityGrams)
            .LessThanOrEqualTo(MaxCustomQuantityGrams)
            .When(x => x.CustomQuantityGrams.HasValue)
            .WithMessage($"{nameof(RecordConsumptionFromPresetCommand.CustomQuantityGrams)} must be between {MinCustomQuantityGrams} and {MaxCustomQuantityGrams} grams when provided");

        RuleFor(x => x.ConsumedAt)
            .Must(date => date <= _clock.UtcNow.AddMinutes(ClockSkewToleranceMinutes))
            .When(x => x.ConsumedAt.HasValue)
            .WithMessage($"{nameof(RecordConsumptionFromPresetCommand.ConsumedAt)} cannot be in the future");
    }
}