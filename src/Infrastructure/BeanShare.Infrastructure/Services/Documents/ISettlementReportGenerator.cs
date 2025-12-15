using BeanShare.Application.Features.Settlement.Dtos;

namespace BeanShare.Infrastructure.Services.Documents;

/// <summary>
/// Generates settlement reports in various formats.
/// </summary>
public interface ISettlementReportGenerator
{
    /// <summary>
    /// Generates a PDF settlement report.
    /// </summary>
    /// <param name="data">Settlement report data.</param>
    /// <returns>PDF document as byte array.</returns>
    byte[] GeneratePdf(SettlementReportData data);

    /// <summary>
    /// Generates an Excel settlement report.
    /// </summary>
    /// <param name="data">Settlement report data.</param>
    /// <returns>Excel document as byte array.</returns>
    byte[] GenerateExcel(SettlementReportData data);
}
