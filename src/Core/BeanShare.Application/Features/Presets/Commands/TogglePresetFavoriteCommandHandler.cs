using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Repositories;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Presets.Commands;
public sealed class TogglePresetFavoriteCommandHandler : IRequestHandler<TogglePresetFavoriteCommand, Result>
{
    private readonly IUserPresetFavoriteRepository _favoriteRepository;
    private readonly IGlobalPresetRepository _globalPresetRepository;
    private readonly IPresetRecipeRepository _presetRecipeRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public TogglePresetFavoriteCommandHandler(
        IUserPresetFavoriteRepository favoriteRepository,
        IGlobalPresetRepository globalPresetRepository,
        IPresetRecipeRepository presetRecipeRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext,
        IClock clock)
    {
        _favoriteRepository = favoriteRepository;
        _globalPresetRepository = globalPresetRepository;
        _presetRecipeRepository = presetRecipeRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result> Handle(TogglePresetFavoriteCommand request, CancellationToken cancellationToken)
    {
        if (request.GlobalPresetId is null && request.SpacePresetId is null)
        {
            return Result.Failure(Error.ValidationFailure("PresetId", "Either GlobalPresetId or SpacePresetId must be provided"));
        }

        if (request.GlobalPresetId is not null && request.SpacePresetId is not null)
        {
            return Result.Failure(Error.ValidationFailure("PresetId", "Only one of GlobalPresetId or SpacePresetId can be provided"));
        }

        var spaceId = new SpaceId(request.SpaceId);
        var userId = _userContext.CurrentUserId;

        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);
        if (space is null)
        {
            return Result.Failure(Error.NotFound("Space", "Space not found"));
        }

        if (!space.HasMember(userId))
        {
            return Result.Failure(Error.Forbidden("Space", "You are not a member of this space"));
        }

        if (request.GlobalPresetId is not null)
        {
            return await HandleGlobalPresetFavorite(userId, spaceId, request.GlobalPresetId.Value, request.IsFavorite, cancellationToken);
        }
        else
        {
            return await HandleSpacePresetFavorite(userId, spaceId, request.SpacePresetId!.Value, request.IsFavorite, cancellationToken);
        }
    }

    private async Task<Result> HandleGlobalPresetFavorite(
        Domain.Common.UserId userId,
        SpaceId spaceId,
        Guid globalPresetIdValue,
        bool isFavorite,
        CancellationToken cancellationToken)
    {
        var globalPresetId = new GlobalPresetId(globalPresetIdValue);

        var presetExists = await _globalPresetRepository.ExistsAsync(globalPresetId, cancellationToken);
        if (!presetExists)
        {
            return Result.Failure(Error.NotFound("GlobalPreset", "Global preset not found"));
        }

        var existingFavorite = await _favoriteRepository.GetByUserSpaceAndGlobalPresetAsync(userId, spaceId, globalPresetId, cancellationToken);

        if (isFavorite)
        {
            if (existingFavorite is null)
            {
                var maxOrder = await _favoriteRepository.GetMaxDisplayOrderAsync(userId, spaceId, cancellationToken);
                var favorite = UserPresetFavorite.CreateForGlobalPreset(
                    userId,
                    spaceId,
                    globalPresetId,
                    maxOrder + 1,
                    _clock.UtcNow);
                await _favoriteRepository.AddAsync(favorite, cancellationToken);
            }
        }
        else
        {
            if (existingFavorite is not null)
            {
                await _favoriteRepository.DeleteAsync(existingFavorite.Id, cancellationToken);
            }
        }

        return Result.Success();
    }

    private async Task<Result> HandleSpacePresetFavorite(
        Domain.Common.UserId userId,
        SpaceId spaceId,
        Guid spacePresetIdValue,
        bool isFavorite,
        CancellationToken cancellationToken)
    {
        var spacePresetId = new PresetRecipeId(spacePresetIdValue);

        var preset = await _presetRecipeRepository.GetByIdAsync(spacePresetId, cancellationToken);
        if (preset is null)
        {
            return Result.Failure(Error.NotFound("SpacePreset", "Space preset not found"));
        }

        if (preset.SpaceId != spaceId)
        {
            return Result.Failure(Error.Forbidden("SpacePreset", "Preset does not belong to this space"));
        }

        var existingFavorite = await _favoriteRepository.GetByUserSpaceAndSpacePresetAsync(userId, spaceId, spacePresetId, cancellationToken);

        if (isFavorite)
        {
            if (existingFavorite is null)
            {
                var maxOrder = await _favoriteRepository.GetMaxDisplayOrderAsync(userId, spaceId, cancellationToken);
                var favorite = UserPresetFavorite.CreateForSpacePreset(
                    userId,
                    spaceId,
                    spacePresetId,
                    maxOrder + 1,
                    _clock.UtcNow);
                await _favoriteRepository.AddAsync(favorite, cancellationToken);
            }
        }
        else
        {
            if (existingFavorite is not null)
            {
                await _favoriteRepository.DeleteAsync(existingFavorite.Id, cancellationToken);
            }
        }

        return Result.Success();
    }
}
