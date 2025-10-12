using BeanShare.Domain.Common;

namespace BeanShare.Application.Features.Spaces.Dtos
{
    public sealed record MembershipDto
    {
        public required UserId UserId { get; init; }
        public required string Email { get; init; }
        public required string Role { get; init; }
        public required DateTime JoinedAt { get; init; }

        public string UserName { get; init; } = "Unknown User";
    }
}
