using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.ActiveQrCodes.Dtos;
using BeanShare.Domain.Entities;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.ActiveQrCodes.Queries;

public sealed class GetActiveQrCodesQueryHandler : IRequestHandler<GetActiveQrCodesQuery, Result<IReadOnlyList<ActiveQrCodeDto>>>
{
    private readonly IActiveQrCodeRepository _qrCodeRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;

    public GetActiveQrCodesQueryHandler(
        IActiveQrCodeRepository qrCodeRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext)
    {
        _qrCodeRepository = qrCodeRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<ActiveQrCodeDto>>> Handle(GetActiveQrCodesQuery query, CancellationToken cancellationToken)
    {
        var spaceId = new SpaceId(query.SpaceId);
        var space = await _spaceRepository.GetByIdAsync(spaceId, cancellationToken);

        if (space is null)
        {
            return Result<IReadOnlyList<ActiveQrCodeDto>>.Failure(Error.SpaceNotFound(query.SpaceId));
        }

        if (!space.HasMember(_userContext.CurrentUserId))
        {
            return Result<IReadOnlyList<ActiveQrCodeDto>>.Failure(Error.NotMember());
        }

        var qrCodes = await _qrCodeRepository.GetBySpaceIdAsync(spaceId, cancellationToken);

        var dtos = qrCodes
            .Select(ActiveQrCodeMapper.ToDto)
            .ToList() as IReadOnlyList<ActiveQrCodeDto>;

        return Result<IReadOnlyList<ActiveQrCodeDto>>.Success(dtos);
    }
}
