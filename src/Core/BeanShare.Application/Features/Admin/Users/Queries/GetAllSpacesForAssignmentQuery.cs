using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Admin.Users.Queries;
public sealed record GetAllSpacesForAssignmentQuery : IQuery<Result<IReadOnlyList<SpaceOptionDto>>>;

public sealed record SpaceOptionDto(Guid Id, string Name);