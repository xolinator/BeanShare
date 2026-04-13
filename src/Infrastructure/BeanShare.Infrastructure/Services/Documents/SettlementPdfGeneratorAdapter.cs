using BeanShare.Application.Abstractions;
using BeanShare.Application.Features.Settlement.Dtos;

namespace BeanShare.Infrastructure.Services.Documents;

public sealed class SettlementPdfGeneratorAdapter : ISettlementPdfGenerator
{
    private readonly ISettlementReportGenerator _generator;

    public SettlementPdfGeneratorAdapter(ISettlementReportGenerator generator)
    {
        _generator = generator;
    }

    public byte[] Generate(SettlementReportData data) => _generator.GeneratePdf(data);
}
