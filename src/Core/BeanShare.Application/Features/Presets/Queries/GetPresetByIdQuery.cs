using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Presets.Queries;

public sealed record GetPresetByIdQuery(Guid PresetId) : IQuery<Result<PresetDto>>;