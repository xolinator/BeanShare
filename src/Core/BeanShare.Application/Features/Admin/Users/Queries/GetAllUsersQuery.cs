using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Admin.Users.Dtos;
using BeanShare.Domain.Enums;

namespace BeanShare.Application.Features.Admin.Users.Queries;
public sealed record GetAllUsersQuery(
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    SystemRole? RoleFilter = null,
    bool? ActiveFilter = null) : IQuery<Result<GetAllUsersResult>>;

public sealed record GetAllUsersResult(
    IReadOnlyList<AdminUserDto> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
