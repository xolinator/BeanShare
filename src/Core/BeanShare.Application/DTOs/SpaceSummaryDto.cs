namespace BeanShare.Application.DTOs;

public sealed record SpaceSummaryDto(
    Guid Id,
    string Name,
    int MemberCount,
    string? InviteCode = null
);