using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Spaces.Dtos;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed record DeactivateSpaceCommand(Guid SpaceId) : ICommand<Result<SpaceDto>>;