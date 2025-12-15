using BeanShare.Application.Common;
using MediatR;

namespace BeanShare.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// Command to update the user's profile information.
/// </summary>
/// <param name="Name">The user's display name</param>
/// <param name="PictureUrl">URL to the user's profile picture, or null to clear</param>
public sealed record UpdateProfileCommand(string Name, string? PictureUrl) : IRequest<Result>;
