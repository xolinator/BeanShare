using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class DemoteMemberValidator : AbstractValidator<DemoteMemberCommand>
{
    public DemoteMemberValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage($"{nameof(DemoteMemberCommand.SpaceId)} is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage($"{nameof(DemoteMemberCommand.UserId)} is required");
    }
}
