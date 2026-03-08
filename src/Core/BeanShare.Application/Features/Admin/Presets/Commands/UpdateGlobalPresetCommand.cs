using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Admin.Presets.Commands;
public sealed record UpdateGlobalPresetCommand(
    Guid Id,
    string Name,
    string DefaultCoffeeType,
    string DefaultPreparation,
    decimal DefaultGrams,
    string? Description,
    int DisplayOrder) : ICommand<Result>;
