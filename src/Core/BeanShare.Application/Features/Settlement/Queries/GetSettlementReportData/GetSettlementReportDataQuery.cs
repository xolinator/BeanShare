using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Settlement.Queries.GetSettlementReportData;
public sealed record GetSettlementReportDataQuery(SettlementId SettlementId) : IQuery<Result<SettlementReportData>>;
