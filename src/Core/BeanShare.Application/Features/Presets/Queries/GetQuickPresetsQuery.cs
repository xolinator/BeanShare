using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Queries;

/// <summary>
/// Gets the presets available for quick consumption buttons in a space.
/// Returns global presets (filtered by space config) and space-specific presets,
/// with user favorites marked.
/// </summary>
public sealed record GetQuickPresetsQuery(Guid SpaceId) : IQuery<Result<GetQuickPresetsResult>>;

public sealed record GetQuickPresetsResult(IReadOnlyCollection<QuickPresetDto> Presets);

public sealed record QuickPresetDto(
    Guid? GlobalPresetId,
    Guid? SpacePresetId,
    string Name,
    string CoffeeType,
    string Brand,
    string Preparation,
    decimal DefaultGrams,
    string? Description,
    bool IsFavorite,
    bool IsGlobal,
    int DisplayOrder);
