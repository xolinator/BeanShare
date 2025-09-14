using BeanShare.Application.Features.Spaces.Dtos;

namespace BeanShare.Application.Features.Spaces.Queries;

public sealed record GetUserSpacesResult(IReadOnlyList<SpaceSummaryDto> Spaces);