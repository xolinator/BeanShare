using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Admin.Users.Dtos;
using MediatR;

namespace BeanShare.Application.Features.Admin.Users.Queries;
public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<GetAllUsersResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly ISpaceRepository _spaceRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository, ISpaceRepository spaceRepository)
    {
        _userRepository = userRepository;
        _spaceRepository = spaceRepository;
    }

    public async Task<Result<GetAllUsersResult>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var (users, totalCount) = await _userRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.RoleFilter,
            request.ActiveFilter,
            cancellationToken);

        var spaceCounts = await _spaceRepository.GetSpaceCountsForUsersAsync(
            users.Select(u => u.Id), cancellationToken);

        var userDtos = users.Select(user => new AdminUserDto(
            user.Id.Value,
            user.Email,
            user.Name,
            user.PictureUrl,
            user.Provider,
            user.SystemRole,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            user.DeactivatedAt,
            spaceCounts.GetValueOrDefault(user.Id, 0))).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        return Result<GetAllUsersResult>.Success(new GetAllUsersResult(
            userDtos,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages));
    }
}
