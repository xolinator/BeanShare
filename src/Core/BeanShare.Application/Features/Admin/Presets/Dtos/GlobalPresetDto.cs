namespace BeanShare.Application.Features.Admin.Presets.Dtos;
public sealed record GlobalPresetDto(
    Guid Id,
    string Name,
    string DefaultCoffeeType,
    string DefaultPreparation,
    decimal DefaultGrams,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt);
