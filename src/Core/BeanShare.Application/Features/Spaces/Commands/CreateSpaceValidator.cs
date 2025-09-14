using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class CreateSpaceValidator : AbstractValidator<CreateSpaceCommand>
{
    public CreateSpaceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Coffee space name is required")
            .MaximumLength(100)
            .WithMessage("Coffee space name must not exceed 100 characters");
    }
}