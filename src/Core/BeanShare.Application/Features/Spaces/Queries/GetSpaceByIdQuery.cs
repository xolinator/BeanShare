using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Spaces.Dtos;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Spaces.Queries;
public sealed record GetSpaceByIdQuery(SpaceId SpaceId) : IQuery<Result<SpaceDto>>;