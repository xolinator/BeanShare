using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class JoinSpaceValidator : AbstractValidator<JoinSpaceCommand>
{
    public JoinSpaceValidator()
    {
        RuleFor(x => x.InviteCode)
            .NotEmpty()
            .WithMessage("Invite code is required")
            .Matches("^[A-Z0-9]{6,10}$")
            .WithMessage("Invite code format is invalid");
    }
}