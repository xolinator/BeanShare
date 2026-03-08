using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Dtos;
using MediatR;

namespace BeanShare.Application.Features.Settlement.Queries.GetSettlement;

public sealed record GetSettlementQuery(
    Guid SettlementId
) : IRequest<Result<SettlementDto>>;