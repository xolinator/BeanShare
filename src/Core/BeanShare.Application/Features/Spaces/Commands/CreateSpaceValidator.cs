using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class CreateSpaceValidator : AbstractValidator<CreateSpaceCommand>
{
    public CreateSpaceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage($"{nameof(CreateSpaceCommand.Name)} is required")
            .MaximumLength(100)
            .WithMessage($"{nameof(CreateSpaceCommand.Name)} must not exceed 100 characters");
    }
}
