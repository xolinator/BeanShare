using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Queries;

public sealed record GetSpacePresetsQuery(Guid SpaceId) : IQuery<Result<GetSpacePresetsResult>>;

public sealed record GetSpacePresetsResult(IReadOnlyCollection<PresetDto> Presets);

public sealed record PresetDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string Name,
    string CoffeeType,
    string Preparation,
    decimal DefaultGrams,
    string? Notes,
    bool IsShared,
    bool IsOwner,
    DateTime CreatedAt,
    DateTime? LastUsedAt,
    int UsageCount);