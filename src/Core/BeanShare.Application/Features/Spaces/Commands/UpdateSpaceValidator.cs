using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class UpdateSpaceValidator : AbstractValidator<UpdateSpaceCommand>
{
    public UpdateSpaceValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage($"{nameof(UpdateSpaceCommand.SpaceId)} is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage($"{nameof(UpdateSpaceCommand.Name)} is required")
            .MaximumLength(100)
            .WithMessage($"{nameof(UpdateSpaceCommand.Name)} cannot exceed 100 characters")
            .MinimumLength(1)
            .WithMessage($"{nameof(UpdateSpaceCommand.Name)} cannot be empty");
    }
}
