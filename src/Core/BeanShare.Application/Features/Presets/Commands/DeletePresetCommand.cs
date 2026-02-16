using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Commands;
public sealed record DeletePresetCommand(Guid PresetId) : ICommand<Result>;