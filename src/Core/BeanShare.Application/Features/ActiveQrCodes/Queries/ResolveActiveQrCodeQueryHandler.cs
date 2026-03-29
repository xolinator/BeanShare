using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.ActiveQrCodes.Dtos;
using BeanShare.Domain.Entities;
using BeanShare.Domain.Specifications;
using BeanShare.Domain.ValueObjects;
using MediatR;

namespace BeanShare.Application.Features.ActiveQrCodes.Queries;

public sealed class ResolveActiveQrCodeQueryHandler : IRequestHandler<ResolveActiveQrCodeQuery, Result<ActiveQrCodeDto>>
{
    private readonly IActiveQrCodeRepository _qrCodeRepository;

    public ResolveActiveQrCodeQueryHandler(IActiveQrCodeRepository qrCodeRepository)
    {
        _qrCodeRepository = qrCodeRepository;
    }

    public async Task<Result<ActiveQrCodeDto>> Handle(ResolveActiveQrCodeQuery query, CancellationToken cancellationToken)
    {
        var spec = new ActiveQrCodeByIdSpecification(new ActiveQrCodeId(query.QrCodeId));
        var qrCode = await _qrCodeRepository.GetSingleBySpecAsync(spec, cancellationToken);

        if (qrCode is null)
        {
            return Result<ActiveQrCodeDto>.Failure(Error.NotFound("ActiveQrCode", $"QR code {query.QrCodeId} not found"));
        }

        if (!qrCode.IsActive)
        {
            return Result<ActiveQrCodeDto>.Failure(Error.NotFound("ActiveQrCode", $"QR code {query.QrCodeId} is no longer active"));
        }

        return Result<ActiveQrCodeDto>.Success(ActiveQrCodeMapper.ToDto(qrCode));
    }
}
