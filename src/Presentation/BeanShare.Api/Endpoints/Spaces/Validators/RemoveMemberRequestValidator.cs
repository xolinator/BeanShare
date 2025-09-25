using BeanShare.Contracts.Spaces;
using FastEndpoints;
using FluentValidation;

namespace BeanShare.Api.Endpoints.Spaces.Validators;

public sealed class RemoveMemberRequestValidator : Validator<RemoveMemberRequest>
{
    public RemoveMemberRequestValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage("Space ID is required")
            .Must(BeValidGuid)
            .WithMessage("Space ID must be a valid GUID");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required")
            .Must(BeValidGuid)
            .WithMessage("User ID must be a valid GUID");
    }

    private static bool BeValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}