using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.ActiveQrCodes.Dtos;

namespace BeanShare.Application.Features.ActiveQrCodes.Queries;

public sealed record ResolveActiveQrCodeQuery(
    Guid QrCodeId
) : IQuery<Result<ActiveQrCodeDto>>;
