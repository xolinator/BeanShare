using BeanShare.Application.Features.Settlement.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BeanShare.Infrastructure.Services.Documents;

/// <summary>
/// Generates PDF settlement reports using QuestPDF.
/// </summary>
public sealed class SettlementPdfGenerator
{
    static SettlementPdfGenerator()
    {
        // Configure QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Generates a PDF settlement report.
    /// </summary>
    public byte[] GeneratePdf(SettlementReportData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, data));
                page.Content().Element(c => ComposeContent(c, data));
                page.Footer().Element(c => ComposeFooter(c, data));
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, SettlementReportData data)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BeanShare")
                        .FontSize(24)
                        .Bold()
                        .FontColor(Colors.Brown.Darken2);

                    col.Item().Text("Settlement Report")
                        .FontSize(16)
                        .SemiBold();
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text($"Generated: {data.GeneratedAt:dd MMM yyyy}")
                        .FontSize(9);
                    col.Item().Text($"By: {data.GeneratedByName}")
                        .FontSize(9);
                });
            });

            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private void ComposeContent(IContainer container, SettlementReportData data)
    {
        container.PaddingVertical(10).Column(column =>
        {
            column.Item().Element(c => ComposeSpaceInfo(c, data));

            column.Item().PaddingVertical(15).Element(c => ComposeSummary(c, data));

            column.Item().Element(c => ComposeTable(c, data));
        });
    }

    private void ComposeSpaceInfo(IContainer container, SettlementReportData data)
    {
        container.Background(Colors.Grey.Lighten4).Padding(15).Column(column =>
        {
            column.Item().Text(data.SpaceName)
                .FontSize(14)
                .SemiBold();

            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Billing Period: {data.BillingPeriodName}");
            });

            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Period: {data.PeriodStart:dd MMM yyyy} - {data.PeriodEnd:dd MMM yyyy}");
            });
        });
    }

    private void ComposeSummary(IContainer container, SettlementReportData data)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten1).Padding(15).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Total Amount").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text($"{data.TotalAmount:N2} {data.Currency}")
                    .FontSize(18)
                    .Bold()
                    .FontColor(Colors.Brown.Darken2);
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Participants").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text($"{data.TotalParticipants}")
                    .FontSize(18)
                    .Bold();
            });

            row.RelativeItem().Column(col =>
            {
                col.Item().Text("Total Coffee").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text($"{data.TotalCoffeeGrams:N1}g")
                    .FontSize(18)
                    .Bold();
            });

            if (data.TotalMilkMl > 0)
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Total Milk").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().Text($"{data.TotalMilkMl:N1}ml")
                        .FontSize(18)
                        .Bold();
                });
            }
        });
    }

    private void ComposeTable(IContainer container, SettlementReportData data)
    {
        container.Column(outerColumn =>
        {
            outerColumn.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Name
                columns.RelativeColumn(2); // Coffee (g)
                columns.RelativeColumn(2); // Milk (ml)
                columns.RelativeColumn(2); // Share %
                columns.RelativeColumn(2); // Amount Due
                columns.RelativeColumn(2); // Status
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Brown.Darken2).Padding(5)
                    .Text("Member").FontColor(Colors.White).SemiBold();
                header.Cell().Background(Colors.Brown.Darken2).Padding(5)
                    .Text("Coffee (g)").FontColor(Colors.White).SemiBold();
                header.Cell().Background(Colors.Brown.Darken2).Padding(5)
                    .Text("Milk (ml)").FontColor(Colors.White).SemiBold();
                header.Cell().Background(Colors.Brown.Darken2).Padding(5)
                    .Text("Share %").FontColor(Colors.White).SemiBold();
                header.Cell().Background(Colors.Brown.Darken2).Padding(5)
                    .Text("Amount Due").FontColor(Colors.White).SemiBold();
                header.Cell().Background(Colors.Brown.Darken2).Padding(5)
                    .Text("Status").FontColor(Colors.White).SemiBold();
            });

            foreach (var line in data.Lines.OrderByDescending(l => l.AmountDue))
            {
                var bgColor = line.IsPaid ? Colors.Green.Lighten5 : Colors.White;

                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).Column(col =>
                    {
                        col.Item().Text(line.UserName).SemiBold();
                        col.Item().Text(line.UserEmail).FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Text($"{line.CoffeeGrams:N1}");

                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Text(line.MilkMl.HasValue ? $"{line.MilkMl:N1}" : "-");

                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Text($"{line.ConsumptionPercentage:N1}%");

                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Text($"{line.AmountDue:N2} {data.Currency}").SemiBold();

                var statusColor = line.IsPaid ? Colors.Green.Darken1 : Colors.Orange.Darken1;
                var statusText = line.IsPaid ? "Paid" : "Pending";
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignCenter().Text(statusText).FontColor(statusColor).SemiBold();
            }
        });

            var paidLines = data.Lines.Where(l => l.IsPaid && l.ConfirmedAt.HasValue).ToList();
            if (paidLines.Any())
            {
                outerColumn.Item().PaddingTop(15).Column(column =>
                {
                    column.Item().Text("Payment Confirmations")
                        .FontSize(11)
                        .SemiBold();

                    column.Item().PaddingTop(5).Column(detailCol =>
                    {
                        foreach (var line in paidLines.OrderBy(l => l.ConfirmedAt))
                        {
                            detailCol.Item().Text($"  {line.UserName}: Confirmed by {line.ConfirmedByName ?? "Self"} on {line.ConfirmedAt:dd MMM yyyy HH:mm}")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                        }
                    });
                });
            }
        });
    }

    private void ComposeFooter(IContainer container, SettlementReportData data)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                    text.Span("Generated by BeanShare  |  ");
                    text.Span($"{data.GeneratedAt:dd MMM yyyy HH:mm}");
                });

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Darken1));
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        });
    }
}
