using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Repositories;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Presets.Queries;
public sealed class GetSpaceGlobalPresetsQueryHandler : IRequestHandler<GetSpaceGlobalPresetsQuery, Result<GetSpaceGlobalPresetsResult>>
{
    private readonly IGlobalPresetRepository _globalPresetRepository;
    private readonly ISpaceGlobalPresetConfigRepository _spaceConfigRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;

    public GetSpaceGlobalPresetsQueryHandler(
        IGlobalPresetRepository globalPresetRepository,
        ISpaceGlobalPresetConfigRepository spaceConfigRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext)
    {
        _globalPresetRepository = globalPresetRepository;
        _spaceConfigRepository = spaceConfigRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetSpaceGlobalPresetsResult>> Handle(GetSpaceGlobalPresetsQuery request, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var userId = _userContext.CurrentUserId;

        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);
        if (space is null)
        {
            return Result<GetSpaceGlobalPresetsResult>.Failure(Error.NotFound("Space", "Space not found"));
        }

        if (!space.HasMember(userId))
        {
            return Result<GetSpaceGlobalPresetsResult>.Failure(Error.Forbidden("Space", "You are not a member of this space"));
        }

        var globalPresets = await _globalPresetRepository.GetAllActiveAsync(cancellationToken);

        var disabledPresetIds = await _spaceConfigRepository.GetDisabledPresetIdsForSpaceAsync(spaceId, cancellationToken);
        var disabledSet = new HashSet<Guid>(disabledPresetIds.Select(id => id.Value));

        var results = globalPresets.Select(preset => new SpaceGlobalPresetDto(
            GlobalPresetId: preset.Id.Value,
            Name: preset.Name,
            CoffeeType: preset.DefaultCoffeeType,
            Preparation: preset.DefaultPreparation,
            DefaultGrams: preset.DefaultGrams.Grams,
            Description: preset.Description,
            IsEnabled: !disabledSet.Contains(preset.Id.Value),
            DisplayOrder: preset.DisplayOrder
        )).OrderBy(p => p.DisplayOrder).ToList();

        return Result<GetSpaceGlobalPresetsResult>.Success(new GetSpaceGlobalPresetsResult(results));
    }
}
