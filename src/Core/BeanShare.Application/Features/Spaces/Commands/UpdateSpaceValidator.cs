using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class UpdateSpaceValidator : AbstractValidator<UpdateSpaceCommand>
{
    public UpdateSpaceValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage("Space ID is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Space name is required")
            .MaximumLength(100)
            .WithMessage("Space name cannot exceed 100 characters")
            .MinimumLength(1)
            .WithMessage("Space name cannot be empty");
    }
}