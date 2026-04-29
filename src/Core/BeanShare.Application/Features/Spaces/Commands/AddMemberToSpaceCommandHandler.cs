using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class AddMemberToSpaceCommandHandler : IRequestHandler<AddMemberToSpaceCommand, Result>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClock _clock;

    public AddMemberToSpaceCommandHandler(
        ISpaceRepository spaceRepository,
        IUserRepository userRepository,
        IClock clock)
    {
        _spaceRepository = spaceRepository;
        _userRepository = userRepository;
        _clock = clock;
    }

    public async Task<Result> Handle(AddMemberToSpaceCommand request, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(request.SpaceId);
        var userId = new UserId(request.UserId);

        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);
        if (space is null)
            return Result.Failure(Error.SpaceNotFound(request.SpaceId));

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.UserNotFound(request.UserId));

        if (space.HasMember(userId))
            return Result.Failure(Error.AlreadySpaceMember(request.SpaceId, request.UserId));

        space.Join(userId, _clock);
        await _spaceRepository.UpdateAsync(space, cancellationToken);

        return Result.Success();
    }
}
