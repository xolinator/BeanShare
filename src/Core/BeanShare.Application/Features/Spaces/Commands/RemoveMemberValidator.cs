using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class RemoveMemberValidator : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage($"{nameof(RemoveMemberCommand.SpaceId)} is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage($"{nameof(RemoveMemberCommand.UserId)} is required");
    }
}
