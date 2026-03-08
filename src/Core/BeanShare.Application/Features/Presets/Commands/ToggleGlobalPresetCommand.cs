using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Commands;

/// <summary>
/// Command to enable/disable a global preset for a specific space.
/// Only space admins can perform this action.
/// </summary>
public sealed record ToggleGlobalPresetCommand(
    Guid SpaceId,
    Guid GlobalPresetId,
    bool IsEnabled) : ICommand<Result>;
