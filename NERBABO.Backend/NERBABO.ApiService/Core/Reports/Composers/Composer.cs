using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Sessions.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class Composer : IComposer
{
    public Document ComposeSessionsTimeline(List<Session> sessions, CourseAction action)
    {
        // Generate PDF using existing logic
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Row(row =>

                    row.RelativeItem().Column(column =>
                        {
                            column.Item().Text($"Cronograma - {action.Course.Title}")
                                .FontSize(14).FontFamily("Arial");
                            column.Item().Text(action.Course.Frame.OperationType)
                                .FontSize(10).FontFamily("Arial");
                        })


                );
                page.Content().Element(container => container.Column(column =>
                {
                    // Action details section
                    column.Item().PaddingTop(20).Column(details =>
                    {
                        details.Item().PaddingBottom(5).Text($"Operação: {action.Course.Frame.Operation}")
                            .FontSize(8).FontFamily("Arial");
                        details.Item().PaddingBottom(5).Text($"Tipologia de intervenção: {action.Course.Frame.InterventionType}")
                            .FontSize(8).FontFamily("Arial");
                        details.Item().PaddingBottom(5).Text($"Total de Horas: {action.Course.TotalDuration}")
                            .FontSize(8).FontFamily("Arial");
                        details.Item().PaddingBottom(5).Text($"Ação: {action.Title}")
                            .FontSize(8).FontFamily("Arial");
                        details.Item().PaddingBottom(5).Text($"Horário: {action.AllDiferentSessionTimes()}")
                            .FontSize(8).FontFamily("Arial");
                        details.Item().PaddingBottom(5).Text($"Morada do Local de realização: {action.Address}")
                            .FontSize(8).FontFamily("Arial");
                        details.Item().PaddingBottom(5).Text($"Formador: {action.AllDiferentSessionTeachers()}")
                            .FontSize(8).FontFamily("Arial");
                    });

                    // Sessions table section
                    column.Item().PaddingTop(30).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(60);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Data").FontSize(8).FontFamily("Arial").SemiBold();
                            header.Cell().Element(CellStyle).Text("Módulo").FontSize(8).FontFamily("Arial").SemiBold();
                            header.Cell().Element(CellStyle).Text("Horário").FontSize(8).FontFamily("Arial").SemiBold();
                            header.Cell().Element(CellStyle).Text("Duração").FontSize(8).FontFamily("Arial").SemiBold();
                        });

                        foreach (var session in sessions)
                        {
                            table.Cell().Element(CellStyle).Text(session.ScheduledDate.ToString("dd/MM/yy")).FontSize(7).FontFamily("Arial");
                            table.Cell().Element(CellStyle).Text(session.ModuleTeaching.Module?.Name ?? "N/A").FontSize(7).FontFamily("Arial");
                            table.Cell().Element(CellStyle).Text(session.Time).FontSize(7).FontFamily("Arial");
                            table.Cell().Element(CellStyle).Text($"{session.DurationHours:F1}h").FontSize(7).FontFamily("Arial");
                        }

                        static IContainer CellStyle(IContainer container) =>
                            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
                    });
                }));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span(" / ");
                });
            });
        });

    }


    // TODO: Refactor to display small logo
    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("NERBA - Sistema de Gestão")
                    .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
            });

            row.ConstantItem(100).AlignRight()
                .Text($"Data: {DateTime.Now:dd/MM/yyyy}");
        });
    }
}