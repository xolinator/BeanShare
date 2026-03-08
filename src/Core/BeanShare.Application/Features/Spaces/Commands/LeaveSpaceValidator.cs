using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class LeaveSpaceValidator : AbstractValidator<LeaveSpaceCommand>
{
    public LeaveSpaceValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage($"{nameof(LeaveSpaceCommand.SpaceId)} is required");
    }
}
