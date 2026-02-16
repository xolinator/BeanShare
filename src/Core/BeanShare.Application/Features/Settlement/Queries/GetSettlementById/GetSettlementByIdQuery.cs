using BeanShare.Application.Abstractions;
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Application.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Settlement.Queries.GetSettlementById;
public sealed record GetSettlementByIdQuery(SettlementId Id) : IQuery<Result<SettlementDto>>;