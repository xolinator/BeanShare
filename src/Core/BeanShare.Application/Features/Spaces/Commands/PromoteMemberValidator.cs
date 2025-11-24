using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class PromoteMemberValidator : AbstractValidator<PromoteMemberCommand>
{
    public PromoteMemberValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage($"{nameof(PromoteMemberCommand.SpaceId)} is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage($"{nameof(PromoteMemberCommand.UserId)} is required");
    }
}
