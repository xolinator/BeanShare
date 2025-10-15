using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.Aggregates.Space;
using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.Spaces.Commands;

public sealed class CreateSpaceCommandHandler : IRequestHandler<CreateSpaceCommand, Result<CreateSpaceResult>>
{
    private readonly ISpaceRepository _spaceRepository;
    private readonly IInviteCodeGenerator _inviteCodeGenerator;
    private readonly IUserContext _userContext;
    private readonly IClock _clock;

    public CreateSpaceCommandHandler(
        ISpaceRepository spaceRepository,
        IInviteCodeGenerator inviteCodeGenerator,
        IUserContext userContext,
        IClock clock)
    {
        _spaceRepository = spaceRepository;
        _inviteCodeGenerator = inviteCodeGenerator;
        _userContext = userContext;
        _clock = clock;
    }

    public async Task<Result<CreateSpaceResult>> Handle(CreateSpaceCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<CreateSpaceResult>.Failure(Error.ValidationFailure(nameof(request.Name), "Space name is required"));
        }

        try
        {
            var spaceId = SpaceId.New();
            var inviteCode = await _inviteCodeGenerator.GenerateAsync(cancellationToken);

            var space = Space.Create(
                spaceId,
                request.Name.Trim(),
                _userContext.CurrentUserId,
                inviteCode,
                _clock);

            await _spaceRepository.AddAsync(space, cancellationToken);

            return Result<CreateSpaceResult>.Success(new CreateSpaceResult(
                spaceId.Value,
                inviteCode.Value));
        }
        catch (ArgumentException ex)
        {
            return Result<CreateSpaceResult>.Failure(Error.ValidationFailure(nameof(Space), ex.Message));
        }
    }
}