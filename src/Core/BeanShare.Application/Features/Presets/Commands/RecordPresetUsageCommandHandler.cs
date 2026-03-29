using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Presets.Commands;
public sealed class RecordPresetUsageCommandHandler : IRequestHandler<RecordPresetUsageCommand, Result>
{
    private readonly IPresetRecipeRepository _presetRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public RecordPresetUsageCommandHandler(
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

    public async Task<Result> Handle(RecordPresetUsageCommand request, CancellationToken cancellationToken)
    {
        var presetId = new PresetRecipeId(request.PresetId);
        var preset = await _presetRepository.GetByIdAsync(presetId, cancellationToken);

        if (preset is null)
        {
            return Result.Failure(Error.NotFound("PresetRecipe", "Preset not found"));
        }

        var space = await _spaceRepository.GetByIdAsync(preset.SpaceId, cancellationToken);
        if (space is null || !space.HasMember(_userContext.CurrentUserId))
        {
            return Result.Failure(Error.Forbidden("PresetRecipe", "You do not have access to this preset"));
        }

        if (preset.UserId != _userContext.CurrentUserId && !preset.IsShared)
        {
            return Result.Failure(Error.Forbidden("PresetRecipe", "You do not have access to this preset"));
        }

        preset.RecordUsage(_clock.UtcNow);
        await _presetRepository.UpdateAsync(preset, cancellationToken);

        return Result.Success();
    }
}