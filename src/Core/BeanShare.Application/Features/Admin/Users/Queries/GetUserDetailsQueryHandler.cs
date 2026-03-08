using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Admin.Users.Dtos;
using BeanShare.Domain.Common;
using BeanShare.Domain.Specifications;
using MediatR;

namespace BeanShare.Application.Features.Admin.Users.Queries;
public sealed class GetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, Result<AdminUserDetailDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ISpaceRepository _spaceRepository;

    public GetUserDetailsQueryHandler(IUserRepository userRepository, ISpaceRepository spaceRepository)
    {
        _userRepository = userRepository;
        _spaceRepository = spaceRepository;
    }

    public async Task<Result<AdminUserDetailDto>> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<AdminUserDetailDto>.Failure(Error.NotFound("User.NotFound", "User not found"));
        }

        var specification = new SpacesWithUserMembershipSpecification(userId);
        var spaces = await _spaceRepository.GetBySpecAsync(specification, cancellationToken);

        var memberships = spaces.Select(space =>
        {
            var membership = space.Members.FirstOrDefault(m => m.UserId == userId);
            if (membership is null) return null;
            return new UserSpaceMembershipDto(
                space.Id.Value,
                space.Name,
                membership.Role,
                membership.JoinedAt);
        }).Where(m => m is not null).Cast<UserSpaceMembershipDto>().ToList();

        var dto = new AdminUserDetailDto(
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
            user.PreferredCurrencyCode,
            memberships);

        return Result<AdminUserDetailDto>.Success(dto);
    }
}
