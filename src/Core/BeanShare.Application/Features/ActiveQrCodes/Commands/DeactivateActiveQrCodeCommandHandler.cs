using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.ActiveQrCodes.Commands;

public sealed class DeactivateActiveQrCodeCommandHandler : IRequestHandler<DeactivateActiveQrCodeCommand, Result<bool>>
{
    private readonly IActiveQrCodeRepository _qrCodeRepository;
    private readonly ISpaceRepository _spaceRepository;
    private readonly IUserContext _userContext;

    public DeactivateActiveQrCodeCommandHandler(
        IActiveQrCodeRepository qrCodeRepository,
        ISpaceRepository spaceRepository,
        IUserContext userContext)
    {
        _qrCodeRepository = qrCodeRepository;
        _spaceRepository = spaceRepository;
        _userContext = userContext;
    }

    public async Task<Result<bool>> Handle(DeactivateActiveQrCodeCommand command, CancellationToken cancellationToken)
    {
        var qrCodeId = new ActiveQrCodeId(command.QrCodeId);
        var qrCode = await _qrCodeRepository.GetByIdAsync(qrCodeId, cancellationToken);

        if (qrCode is null)
        {
            return Result<bool>.Failure(Error.NotFound("ActiveQrCode", $"QR code {command.QrCodeId} not found"));
        }

        var space = await _spaceRepository.GetByIdAsync(qrCode.SpaceId, cancellationToken);

        if (space is null)
        {
            return Result<bool>.Failure(Error.SpaceNotFound(qrCode.SpaceId.Value));
        }

        if (!space.HasMember(_userContext.CurrentUserId))
        {
            return Result<bool>.Failure(Error.NotMember());
        }

        qrCode.Deactivate();
        await _qrCodeRepository.UpdateAsync(qrCode, cancellationToken);

        return Result<bool>.Success(true);
    }
}
