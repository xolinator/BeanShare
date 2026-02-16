using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Admin.Users.Dtos;

namespace BeanShare.Application.Features.Admin.Users.Queries;
public sealed record GetUserDetailsQuery(Guid UserId) : IQuery<Result<AdminUserDetailDto>>;
