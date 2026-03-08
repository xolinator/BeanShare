using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Admin.Presets.Dtos;

namespace BeanShare.Application.Features.Admin.Presets.Queries;
public sealed record GetAllGlobalPresetsQuery(bool IncludeInactive = true) : IQuery<Result<IReadOnlyList<GlobalPresetDto>>>;
