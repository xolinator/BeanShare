using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Spaces.Dtos;

namespace BeanShare.Application.Features.Spaces.Commands;
public sealed record UpdateSpaceCommand(Guid SpaceId, string Name) : ICommand<Result<SpaceDto>>;