using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DocLink.Models.Configuration;
using DocLinkDocument = DocLink.Models.Document;

namespace DocLink.Services;

public class PdfService : IPdfService
{
    private readonly CompanyBranding _branding;

    public PdfService(IOptions<CompanyBranding> branding)
    {
        _branding = branding.Value;
    }

    public byte[] GenerateReceiptPdf(DocLinkDocument document)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontColor(Colors.Black));

                page.Header().Element(c => ComposeHeader(c, document));
                page.Content().Element(c => ComposeContent(c, document));
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    void ComposeHeader(IContainer container, DocLinkDocument document)
    {
        container.Column(column =>
        {
            column.Spacing(5);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(_branding.Name)
                        .SemiBold().FontSize(24).FontColor(Colors.Blue.Medium);

                    if (!string.IsNullOrEmpty(_branding.Address))
                        col.Item().Text(_branding.Address).FontSize(10).FontColor(Colors.Grey.Medium);

                    if (!string.IsNullOrEmpty(_branding.Phone))
                        col.Item().Text(_branding.Phone).FontSize(10).FontColor(Colors.Grey.Medium);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text(document.Title).SemiBold().FontSize(18);
                    col.Item().Text($"#{document.DocumentNumber}").FontSize(12);
                });
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    void ComposeContent(IContainer container, DocLinkDocument document)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Bill To:").SemiBold().FontSize(11);
                    col.Item().PaddingTop(3).Text(document.CustomerName).FontSize(11);

                    if (!string.IsNullOrEmpty(document.Address))
                        col.Item().Text(document.Address).FontSize(11);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"Date: {document.Date:MMMM dd, yyyy}").FontSize(11);
                    if (!string.IsNullOrEmpty(document.ReferenceNumber))
                        col.Item().Text($"Reference: {document.ReferenceNumber}").FontSize(11);
                    col.Item().Text($"Status: {document.Status}").FontSize(11);
                });
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Medium).Padding(5)
                        .Text("Description").FontSize(11).FontColor(Colors.White).SemiBold();
                    header.Cell().Background(Colors.Blue.Medium).Padding(5)
                        .AlignCenter().Text("Qty").FontSize(11).FontColor(Colors.White).SemiBold();
                    header.Cell().Background(Colors.Blue.Medium).Padding(5)
                        .AlignRight().Text("Amount").FontSize(11).FontColor(Colors.White).SemiBold();

                    header.Cell().ColumnSpan(3).PaddingTop(3).BorderBottom(1).BorderColor(Colors.Black);
                });

                table.Cell().PaddingVertical(4).PaddingHorizontal(5)
                    .Text(document.Notes ?? document.Title).FontSize(11);
                table.Cell().PaddingVertical(4).AlignCenter()
                    .Text("1").FontSize(11);
                table.Cell().PaddingVertical(4).AlignRight()
                    .Text($"{document.Amount:N2}").FontSize(11);
            });

            column.Item().PaddingVertical(3).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().AlignRight().Column(col =>
            {
                col.Spacing(3);
                col.Item().Text(text =>
                {
                    text.Span("Total: ").SemiBold().FontSize(14);
                    text.Span($"{document.Amount:N2}").FontSize(14);
                });
            });

            if (!string.IsNullOrEmpty(document.Notes))
            {
                column.Item().PaddingTop(10).Column(col =>
                {
                    col.Item().Text("Notes:").SemiBold().FontSize(11);
                    col.Item().Text(document.Notes).FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            }
        });
    }

    void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Column(col =>
        {
            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            col.Item().PaddingTop(5).Text("Thank you for your business!")
                .FontSize(10).FontColor(Colors.Grey.Medium).Italic();
            col.Item().Text("This document was generated electronically.")
                .FontSize(8).FontColor(Colors.Grey.Lighten1);
        });
    }
}
