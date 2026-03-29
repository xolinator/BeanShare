using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Entities;
using MediatR;

namespace BeanShare.Application.Features.Presets.Queries;
public sealed class GetPresetByIdQueryHandler : IRequestHandler<GetPresetByIdQuery, Result<PresetDto>>
{
    private readonly IPresetRecipeRepository _presetRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;

    public GetPresetByIdQueryHandler(
        IPresetRecipeRepository presetRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext)
    {
        _presetRepository = presetRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
    }

    public async Task<Result<PresetDto>> Handle(GetPresetByIdQuery request, CancellationToken cancellationToken)
    {
        var presetId = new PresetRecipeId(request.PresetId);
        var preset = await _presetRepository.GetByIdAsync(presetId, cancellationToken);

        if (preset is null)
        {
            return Result<PresetDto>.Failure(Error.NotFound("PresetRecipe", "Preset not found"));
        }

        var space = await _spaceRepository.GetByIdAsync(preset.SpaceId, cancellationToken);
        if (space is null || !space.HasMember(_userContext.CurrentUserId))
        {
            return Result<PresetDto>.Failure(Error.Forbidden("PresetRecipe", "You do not have access to this preset"));
        }

        if (preset.UserId != _userContext.CurrentUserId && !preset.IsShared)
        {
            return Result<PresetDto>.Failure(Error.Forbidden("PresetRecipe", "You do not have access to this preset"));
        }

        var userName = $"User {preset.UserId.Value}";

        var presetDto = new PresetDto(
            preset.Id.Value,
            preset.UserId.Value,
            userName,
            preset.Name,
            preset.CoffeeType,
            preset.Preparation,
            preset.DefaultGrams.Grams,
            preset.Notes,
            preset.IsShared,
            preset.UserId == _userContext.CurrentUserId,
            preset.CreatedAt,
            preset.LastUsedAt,
            preset.UsageCount);

        return Result<PresetDto>.Success(presetDto);
    }
}