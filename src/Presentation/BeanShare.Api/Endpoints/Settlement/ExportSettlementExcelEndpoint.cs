using BeanShare.Application.Features.Settlement.Queries.GetSettlementReportData;
using BeanShare.Domain.ValueObjects;
using BeanShare.Infrastructure.Services.Documents;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Settlement;

public sealed class ExportSettlementExcelRequest
{
    public Guid Id { get; init; }
}

public sealed class ExportSettlementExcelEndpoint : Endpoint<ExportSettlementExcelRequest>
{
    private readonly IMediator _mediator;
    private readonly ISettlementReportGenerator _reportGenerator;

    public ExportSettlementExcelEndpoint(IMediator mediator, ISettlementReportGenerator reportGenerator)
    {
        _mediator = mediator;
        _reportGenerator = reportGenerator;
    }

    public override void Configure()
    {
        Get("/api/settlements/{Id}/export/excel");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Export settlement as Excel";
            s.Description = "Downloads an Excel spreadsheet for the specified settlement";
        });
    }

    public override async Task HandleAsync(ExportSettlementExcelRequest req, CancellationToken ct)
    {
        var query = new GetSettlementReportDataQuery(new SettlementId(req.Id));
        var result = await _mediator.Send(query, ct);

        if (!result.IsSuccess)
        {
            await SendResultAsync(Results.BadRequest(result.Errors.Any() ? result.Errors.First().Message : "Failed to get settlement data"));
            return;
        }

        var excelBytes = _reportGenerator.GenerateExcel(result.Value);
        var fileName = $"Settlement_{result.Value.BillingPeriodName}_{DateTime.UtcNow:yyyyMMdd}.xlsx";

        HttpContext.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
        await SendBytesAsync(excelBytes, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", cancellation: ct);
    }
}
