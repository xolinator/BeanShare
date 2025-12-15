using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Repositories;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Presets.Queries;

public sealed class GetQuickPresetsQueryHandler : IRequestHandler<GetQuickPresetsQuery, Result<GetQuickPresetsResult>>
{
    private readonly IGlobalPresetRepository _globalPresetRepository;
    private readonly ISpaceGlobalPresetConfigRepository _spaceConfigRepository;
    private readonly IPresetRecipeRepository _presetRecipeRepository;
    private readonly IUserPresetFavoriteRepository _userFavoriteRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;

    public GetQuickPresetsQueryHandler(
        IGlobalPresetRepository globalPresetRepository,
        ISpaceGlobalPresetConfigRepository spaceConfigRepository,
        IPresetRecipeRepository presetRecipeRepository,
        IUserPresetFavoriteRepository userFavoriteRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext)
    {
        _globalPresetRepository = globalPresetRepository;
        _spaceConfigRepository = spaceConfigRepository;
        _presetRecipeRepository = presetRecipeRepository;
        _userFavoriteRepository = userFavoriteRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetQuickPresetsResult>> Handle(GetQuickPresetsQuery request, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var userId = _userContext.CurrentUserId;

        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);
        if (space is null)
        {
            return Result<GetQuickPresetsResult>.Failure(Error.NotFound("Space", "Space not found"));
        }

        if (!space.HasMember(userId))
        {
            return Result<GetQuickPresetsResult>.Failure(Error.Forbidden("Space", "You are not a member of this space"));
        }

        var globalPresets = await _globalPresetRepository.GetAllActiveAsync(cancellationToken);

        var disabledPresetIds = await _spaceConfigRepository.GetDisabledPresetIdsForSpaceAsync(spaceId, cancellationToken);
        var disabledSet = new HashSet<Guid>(disabledPresetIds.Select(id => id.Value));

        var userFavorites = await _userFavoriteRepository.GetByUserAndSpaceAsync(userId, spaceId, cancellationToken);
        var favoriteGlobalPresetIds = new HashSet<Guid>(userFavorites
            .Where(f => f.GlobalPresetId is not null)
            .Select(f => f.GlobalPresetId!.Value));
        var favoriteSpacePresetIds = new HashSet<Guid>(userFavorites
            .Where(f => f.PresetRecipeId is not null)
            .Select(f => f.PresetRecipeId!.Value));

        var spacePresets = await _presetRecipeRepository.GetBySpaceIdAsync(spaceId, sharedOnly: true, cancellationToken);

        var results = new List<QuickPresetDto>();

        foreach (var preset in globalPresets)
        {
            if (disabledSet.Contains(preset.Id.Value))
                continue;

            results.Add(new QuickPresetDto(
                GlobalPresetId: preset.Id.Value,
                SpacePresetId: null,
                Name: preset.Name,
                CoffeeType: preset.DefaultCoffeeType,
                Brand: preset.DefaultBrand,
                Preparation: preset.DefaultPreparation,
                DefaultGrams: preset.DefaultGrams.Grams,
                Description: preset.Description,
                IsFavorite: favoriteGlobalPresetIds.Contains(preset.Id.Value),
                IsGlobal: true,
                DisplayOrder: preset.DisplayOrder));
        }

        var spacePresetOrder = 1000;
        foreach (var preset in spacePresets)
        {
            results.Add(new QuickPresetDto(
                GlobalPresetId: null,
                SpacePresetId: preset.Id.Value,
                Name: preset.Name,
                CoffeeType: preset.CoffeeType,
                Brand: preset.Brand,
                Preparation: preset.Preparation,
                DefaultGrams: preset.DefaultGrams.Grams,
                Description: preset.Notes,
                IsFavorite: favoriteSpacePresetIds.Contains(preset.Id.Value),
                IsGlobal: false,
                DisplayOrder: spacePresetOrder++));
        }

        var sortedResults = results
            .OrderByDescending(p => p.IsFavorite)
            .ThenBy(p => p.DisplayOrder)
            .ToList();

        return Result<GetQuickPresetsResult>.Success(new GetQuickPresetsResult(sortedResults));
    }
}
