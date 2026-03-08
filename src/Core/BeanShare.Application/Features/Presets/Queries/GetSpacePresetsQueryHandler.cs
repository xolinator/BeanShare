using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.Repositories;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Presets.Queries;

public sealed class GetSpacePresetsQueryHandler : IRequestHandler<GetSpacePresetsQuery, Result<GetSpacePresetsResult>>
{
    private readonly IPresetRecipeRepository _presetRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;

    public GetSpacePresetsQueryHandler(
        IPresetRecipeRepository presetRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext)
    {
        _presetRepository = presetRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetSpacePresetsResult>> Handle(GetSpacePresetsQuery request, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);

        if (space is null)
        {
            return Result<GetSpacePresetsResult>.Failure(Error.NotFound("Space", "Space not found"));
        }

        if (!space.HasMember(_userContext.CurrentUserId))
        {
            return Result<GetSpacePresetsResult>.Failure(Error.Forbidden("Space", "You are not a member of this space"));
        }

        var presets = await _presetRepository.GetBySpaceIdAsync(spaceId, ct: cancellationToken);

        var userNames = new Dictionary<UserId, string>();
        foreach (var member in space.Members)
        {
            userNames[member.UserId] = $"User {member.UserId.Value}";
        }

        var presetDtos = presets
            .Where(p => p.UserId == _userContext.CurrentUserId || p.IsShared)
            .Select(p => new PresetDto(
                p.Id.Value,
                p.UserId.Value,
                userNames.GetValueOrDefault(p.UserId, "Unknown User"),
                p.Name,
                p.CoffeeType,
                p.Preparation,
                p.DefaultGrams.Grams,
                p.Notes,
                p.IsShared,
                p.UserId == _userContext.CurrentUserId,
                p.CreatedAt,
                p.LastUsedAt,
                p.UsageCount))
            .OrderByDescending(p => p.UsageCount)
            .ThenByDescending(p => p.LastUsedAt ?? p.CreatedAt)
            .ToList();

        return Result<GetSpacePresetsResult>.Success(new GetSpacePresetsResult(presetDtos));
    }
}