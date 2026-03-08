using BeanShare.Application.Abstractions;
using BeanShare.Application.Common;
using BeanShare.Application.Features.Settlement.Dtos;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Application.Features.Settlement.Queries.GetSettlementReportData;

/// <summary>
/// Query to get settlement data for report generation.
/// </summary>
public sealed record GetSettlementReportDataQuery(SettlementId SettlementId) : IQuery<Result<SettlementReportData>>;
