using AIJobHelper.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AIJobHelper.Infrastructure.Pdf;

public class QuestPdfGenerator : IPdfGenerator
{
    public byte[] Generate(string header, string body, string footer, string fileName)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(ComposeHeader(header));
                page.Content().Element(ComposeBody(body));
                page.Footer().Element(ComposeFooter(footer));
            });
        });

        return document.GeneratePdf();
    }

    private static Action<IContainer> ComposeHeader(string header) => container =>
    {
        if (string.IsNullOrWhiteSpace(header)) return;

        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(8).Column(col =>
        {
            foreach (var line in SplitLines(header))
                col.Item().Text(line).FontSize(11);
        });
    };

    private static Action<IContainer> ComposeBody(string body) => container =>
    {
        container.PaddingVertical(16).Column(col =>
        {
            col.Spacing(10);
            foreach (var paragraph in SplitParagraphs(body))
                col.Item().Text(paragraph).FontSize(11).LineHeight(1.5f);
        });
    };

    private static Action<IContainer> ComposeFooter(string footer) => container =>
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(8).Column(col =>
        {
            if (!string.IsNullOrWhiteSpace(footer))
            {
                foreach (var line in SplitLines(footer))
                    col.Item().Text(line).FontSize(11);
            }
            else
            {
                col.Item().AlignRight().Text(text =>
                {
                    text.Span("Page ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Medium);
                });
            }
        });
    };

    private static IEnumerable<string> SplitLines(string text) =>
        text.Split('\n', StringSplitOptions.None).Select(l => l.TrimEnd());

    private static IEnumerable<string> SplitParagraphs(string text) =>
        text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Replace("\r\n", " ").Replace("\n", " ").Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p));
}
