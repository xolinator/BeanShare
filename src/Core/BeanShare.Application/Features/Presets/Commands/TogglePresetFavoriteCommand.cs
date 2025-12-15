using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Commands;

/// <summary>
/// Command to toggle a preset as a favorite for the current user.
/// Works with both global presets and space-specific presets.
/// </summary>
public sealed record TogglePresetFavoriteCommand(
    Guid SpaceId,
    Guid? GlobalPresetId,
    Guid? SpacePresetId,
    bool IsFavorite) : ICommand<Result>;
