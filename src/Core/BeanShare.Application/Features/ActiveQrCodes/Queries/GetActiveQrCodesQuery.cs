using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Common.Authorization;
using BeanShare.Application.Features.ActiveQrCodes.Dtos;

namespace BeanShare.Application.Features.ActiveQrCodes.Queries;

[RequireSpaceMember("SpaceId")]
public sealed record GetActiveQrCodesQuery(
    Guid SpaceId
) : IAuthorize, IQuery<Result<IReadOnlyList<ActiveQrCodeDto>>>;
