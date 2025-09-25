using BeanShare.Contracts.Spaces;
using FastEndpoints;
using FluentValidation;

namespace BeanShare.Api.Endpoints.Spaces.Validators;

public sealed class UpdateSpaceRequestValidator : Validator<UpdateSpaceRequest>
{
    public UpdateSpaceRequestValidator()
    {
        RuleFor(x => x.SpaceId)
            .NotEmpty()
            .WithMessage("Space ID is required")
            .Must(BeValidGuid)
            .WithMessage("Space ID must be a valid GUID");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Space name is required")
            .MaximumLength(100)
            .WithMessage("Space name cannot exceed 100 characters");
    }

    private static bool BeValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}