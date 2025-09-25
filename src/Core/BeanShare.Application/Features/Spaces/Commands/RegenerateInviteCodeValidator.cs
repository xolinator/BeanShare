using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class RegenerateInviteCodeValidator : AbstractValidator<RegenerateInviteCodeCommand>
{
    public RegenerateInviteCodeValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage("Space ID is required");
    }
}