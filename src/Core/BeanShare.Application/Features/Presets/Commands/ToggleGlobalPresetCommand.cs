using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Commands;
public sealed record ToggleGlobalPresetCommand(
    Guid SpaceId,
    Guid GlobalPresetId,
    bool IsEnabled) : ICommand<Result>;
