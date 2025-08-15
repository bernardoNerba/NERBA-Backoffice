using System;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composer;

public class ComposeReport : IComposeReport
{
    public static Color PrimaryColor = Colors.Green.Darken2;
    
    public void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Footer Gerado.")
                    .FontSize(14);
            });
        });
    }

    public void ComposeHeader(IContainer container, string title)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("NERBA - Sistema de Gestão")
                    .FontSize(20).SemiBold().FontColor(PrimaryColor);
                
                column.Item().Text(title)
                    .FontSize(16).SemiBold();
            });
        });
    }
}
