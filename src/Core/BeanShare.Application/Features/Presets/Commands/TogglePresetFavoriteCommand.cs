using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Commands;
public sealed record TogglePresetFavoriteCommand(
    Guid SpaceId,
    Guid? GlobalPresetId,
    Guid? SpacePresetId,
    bool IsFavorite) : ICommand<Result>;
