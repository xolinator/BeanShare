using BeanShare.Application.Abstractions;
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Application.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Settlement.Queries.GetSpaceSettlements;

public sealed record GetSpaceSettlementsQuery(SpaceId SpaceId) : IQuery<Result<List<SettlementSummaryDto>>>;