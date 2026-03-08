using BeanShare.Application.Features.Settlement.Queries.GetSettlementReportData;
using BeanShare.Domain.ValueObjects;
using BeanShare.Infrastructure.Services.Documents;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Settlement;

public sealed class ExportSettlementPdfRequest
{
    public Guid Id { get; init; }
}

public sealed class ExportSettlementPdfEndpoint : Endpoint<ExportSettlementPdfRequest>
{
    private readonly IMediator _mediator;
    private readonly ISettlementReportGenerator _reportGenerator;

    public ExportSettlementPdfEndpoint(IMediator mediator, ISettlementReportGenerator reportGenerator)
    {
        _mediator = mediator;
        _reportGenerator = reportGenerator;
    }

    public override void Configure()
    {
        Get("/api/settlements/{Id}/export/pdf");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Export settlement as PDF";
            s.Description = "Downloads a PDF report for the specified settlement";
        });
    }

    public override async Task HandleAsync(ExportSettlementPdfRequest req, CancellationToken ct)
    {
        var query = new GetSettlementReportDataQuery(new SettlementId(req.Id));
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            await SendResultAsync(Results.BadRequest(result.Errors.Any() ? result.Errors.First().Message : "Failed to get settlement data"));
            return;
        }

        var pdfBytes = _reportGenerator.GeneratePdf(result.Value);
        var fileName = $"Settlement_{result.Value.BillingPeriodName}_{DateTime.UtcNow:yyyyMMdd}.pdf";

        HttpContext.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
        await SendBytesAsync(pdfBytes, fileName, "application/pdf", cancellation: ct);
    }
}
