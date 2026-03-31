using BeanShare.Application.Common;
using BeanShare.Application.Abstractions;

namespace BeanShare.Application.Features.Users.Commands.UpdateProfile;
public sealed record UpdateProfileCommand(string Name, string? PictureUrl) : ICommand<Result>;
