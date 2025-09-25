using BeanShare.Contracts.Spaces;
using FastEndpoints;
using FluentValidation;

namespace BeanShare.Api.Endpoints.Spaces.Validators;

public sealed class RegenerateInviteCodeRequestValidator : Validator<RegenerateInviteCodeRequest>
{
    public RegenerateInviteCodeRequestValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage("Space ID is required")
            .Must(BeValidGuid)
            .WithMessage("Space ID must be a valid GUID");
    }

    private static bool BeValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}