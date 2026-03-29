using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Presets.Commands;
public sealed class CreatePresetCommandHandler : IRequestHandler<CreatePresetCommand, Result<CreatePresetResult>>
{
    private readonly IPresetRecipeRepository _presetRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public CreatePresetCommandHandler(
        IPresetRecipeRepository presetRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IClock clock)
    {
        _presetRepository = presetRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<CreatePresetResult>> Handle(CreatePresetCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CreatePresetResult>.Failure(Error.ValidationFailure(nameof(request.Name), "Preset name is required"));
        }

        if (string.IsNullOrWhiteSpace(request.CoffeeType))
        {
            return Result<CreatePresetResult>.Failure(Error.ValidationFailure(nameof(request.CoffeeType), "Coffee type is required"));
        }

        if (string.IsNullOrWhiteSpace(request.Preparation))
        {
            return Result<CreatePresetResult>.Failure(Error.ValidationFailure(nameof(request.Preparation), "Preparation method is required"));
        }

        if (request.DefaultGrams <= 0)
        {
            return Result<CreatePresetResult>.Failure(Error.ValidationFailure(nameof(request.DefaultGrams), "Default grams must be positive"));
        }

        try
        {
            var defaultGrams = Weight.FromGrams(request.DefaultGrams);
            var spaceId = new SpaceId(request.SpaceId);

            var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);
            if (space is null)
            {
                return Result<CreatePresetResult>.Failure(Error.NotFound("Space", "Space not found"));
            }

            if (!space.HasMember(_userContext.CurrentUserId))
            {
                return Result<CreatePresetResult>.Failure(Error.Forbidden("Space", "You are not a member of this space"));
            }

            var preset = PresetRecipe.Create(
                _userContext.CurrentUserId,
                spaceId,
                request.Name.Trim(),
                request.CoffeeType.Trim(),
                request.Preparation.Trim(),
                defaultGrams,
                _clock.UtcNow,
                request.Notes?.Trim(),
                request.IsShared);

            await _presetRepository.AddAsync(preset, cancellationToken);

            return Result<CreatePresetResult>.Success(new CreatePresetResult(preset.Id.Value));
        }
        catch (ArgumentException ex)
        {
            return Result<CreatePresetResult>.Failure(Error.ValidationFailure("PresetRecipe", ex.Message));
        }
    }
}