using BeanShare.Application.Features.Settlement.Dtos;

namespace BeanShare.Application.Abstractions;

public interface ISettlementPdfGenerator
{
    byte[] Generate(SettlementReportData data);
}
