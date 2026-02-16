using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Users.Commands.UpdateProfile;
public sealed record UpdateProfileCommand(string Name, string? PictureUrl) : IRequest<Result>;
