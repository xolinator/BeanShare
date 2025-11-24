using FluentValidation;

namespace BeanShare.Application.Features.Consumption.Queries.GetUserConsumptionHistory;

public sealed class GetUserConsumptionHistoryValidator : AbstractValidator<GetUserConsumptionHistoryQuery>
{
    public GetUserConsumptionHistoryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage($"{nameof(GetUserConsumptionHistoryQuery.PageNumber)} must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage($"{nameof(GetUserConsumptionHistoryQuery.PageSize)} must be between 1 and 100");

        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.EndDate!.Value >= x.StartDate!.Value)
                .WithMessage($"{nameof(GetUserConsumptionHistoryQuery.EndDate)} must be greater than or equal to {nameof(GetUserConsumptionHistoryQuery.StartDate)}");
        });

        When(x => x.EndDate.HasValue, () =>
        {
            RuleFor(x => x.EndDate)
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage($"{nameof(GetUserConsumptionHistoryQuery.EndDate)} cannot be in the future");
        });
    }
}