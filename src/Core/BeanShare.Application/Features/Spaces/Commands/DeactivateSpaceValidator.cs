using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class DeactivateSpaceValidator : AbstractValidator<DeactivateSpaceCommand>
{
    public DeactivateSpaceValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage("Space ID is required");
    }
}