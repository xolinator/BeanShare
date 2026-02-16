using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Dtos;
using MediatR;

namespace BeanShare.Application.Features.Settlement.Commands.GenerateSettlement;
public sealed record GenerateSettlementCommand(
    Guid BillingPeriodId
) : IRequest<Result<SettlementDto>>;