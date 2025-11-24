using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Commands;

public sealed record UpdatePresetCommand(
    Guid PresetId,
    string Name,
    string CoffeeType,
    string Brand,
    string Preparation,
    decimal DefaultGrams,
    string? Notes,
    bool IsShared) : ICommand<Result>;