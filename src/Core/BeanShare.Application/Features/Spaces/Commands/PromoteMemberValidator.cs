using FluentValidation;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class PromoteMemberValidator : AbstractValidator<PromoteMemberCommand>
{
    public PromoteMemberValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage("Space ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}