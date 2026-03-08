using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Commands;

public sealed record CreatePresetCommand(
    Guid SpaceId,
    string Name,
    string CoffeeType,
    string Preparation,
    decimal DefaultGrams,
    string? Notes,
    bool IsShared) : ICommand<Result<CreatePresetResult>>;

public sealed record CreatePresetResult(Guid PresetId);