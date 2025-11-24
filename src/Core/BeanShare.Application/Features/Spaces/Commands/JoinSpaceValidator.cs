using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class JoinSpaceValidator : AbstractValidator<JoinSpaceCommand>
{
    public JoinSpaceValidator()
    {
        RuleFor(x => x.InviteCode)
            .NotEmpty()
            .WithMessage($"{nameof(JoinSpaceCommand.InviteCode)} is required")
            .Matches("^[A-Z0-9]{6,10}$")
            .WithMessage($"{nameof(JoinSpaceCommand.InviteCode)} format is invalid");
    }
}
