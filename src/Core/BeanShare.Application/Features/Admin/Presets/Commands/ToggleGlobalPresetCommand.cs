using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Admin.Presets.Commands;
public sealed record ToggleGlobalPresetCommand(Guid Id, bool Activate) : ICommand<Result>;
