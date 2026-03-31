using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Dtos;

namespace BeanShare.Application.Features.Settlement.Commands.GenerateSettlement;
public sealed record GenerateSettlementCommand(
    Guid BillingPeriodId
) : ICommand<Result<SettlementDto>>;