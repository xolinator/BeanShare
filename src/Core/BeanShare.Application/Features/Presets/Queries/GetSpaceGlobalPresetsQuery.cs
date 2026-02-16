using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Queries;
public sealed record GetSpaceGlobalPresetsQuery(Guid SpaceId) : IQuery<Result<GetSpaceGlobalPresetsResult>>;

public sealed record GetSpaceGlobalPresetsResult(IReadOnlyCollection<SpaceGlobalPresetDto> Presets);

public sealed record SpaceGlobalPresetDto(
    Guid GlobalPresetId,
    string Name,
    string CoffeeType,
    string Preparation,
    decimal DefaultGrams,
    string? Description,
    bool IsEnabled,
    int DisplayOrder);
