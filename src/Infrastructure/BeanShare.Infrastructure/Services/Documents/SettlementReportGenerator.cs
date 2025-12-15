using BeanShare.Application.Features.Settlement.Dtos;

namespace BeanShare.Infrastructure.Services.Documents;

/// <summary>
/// Combined settlement report generator implementing the interface.
/// </summary>
public sealed class SettlementReportGenerator : ISettlementReportGenerator
{
    private readonly SettlementPdfGenerator _pdfGenerator;
    private readonly SettlementExcelGenerator _excelGenerator;

    public SettlementReportGenerator()
    {
        _pdfGenerator = new SettlementPdfGenerator();
        _excelGenerator = new SettlementExcelGenerator();
    }

    /// <inheritdoc />
    public byte[] GeneratePdf(SettlementReportData data)
    {
        return _pdfGenerator.GeneratePdf(data);
    }

    /// <inheritdoc />
    public byte[] GenerateExcel(SettlementReportData data)
    {
        return _excelGenerator.GenerateExcel(data);
    }
}
