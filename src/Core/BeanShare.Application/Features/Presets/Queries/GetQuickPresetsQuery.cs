using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Queries;
public sealed record GetQuickPresetsQuery(Guid SpaceId) : IQuery<Result<GetQuickPresetsResult>>;

public sealed record GetQuickPresetsResult(IReadOnlyCollection<QuickPresetDto> Presets);

public sealed record QuickPresetDto(
    Guid? GlobalPresetId,
    Guid? SpacePresetId,
    string Name,
    string CoffeeType,
    string Preparation,
    decimal DefaultGrams,
    string? Description,
    bool IsFavorite,
    bool IsGlobal,
    int DisplayOrder);
