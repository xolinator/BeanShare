using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Commands;

public sealed record RecordPresetUsageCommand(Guid PresetId) : ICommand<Result>;