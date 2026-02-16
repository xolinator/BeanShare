using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;

namespace BeanShare.Application.Features.Spaces.Queries;
public sealed record GetUserSpacesQuery : IQuery<Result<GetUserSpacesResult>>;