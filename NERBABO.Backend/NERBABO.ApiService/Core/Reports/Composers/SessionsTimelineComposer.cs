using Humanizer;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.Sessions.Models;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class SessionsTimelineComposer(HelperComposer helperComposer)
{
    private readonly HelperComposer _helperComposer = helperComposer;

    public async Task<Document> ComposeAsync(List<Session> sessions, CourseAction action, GeneralInfo infos)
    {
        // Pre-load images asynchronously
        var (generalLogoBytes, programLogoBytes, financementLogoBytes) = await _helperComposer
            .LoadLogosAsync(infos.Logo, action.Course.Frame.ProgramLogo, action.Course.Frame.FinancementLogo);
            
        // Generate PDF using existing logic
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Element(container => HelperComposer.ComposeHeader(container, generalLogoBytes, programLogoBytes));
                page.Content().Element(container => ComposeContent(container, action, infos, sessions));
                page.Footer().Element(container => HelperComposer.ComposeFooter(container, financementLogoBytes, $"{infos.Slug} é Entidade Certificada pela DGERT, C61"));
            });
        });
    }

    private static void ComposeContent(IContainer container, CourseAction action, GeneralInfo infos, List<Session> sessions)
    {
        // Agrupar sessões por mês/ano
        var sessionsByMonth = sessions
            .OrderBy(s => s.ScheduledDate)
            .GroupBy(s => new { s.ScheduledDate.Year, s.ScheduledDate.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("pt-PT")),
                Sessions = g.OrderBy(s => s.ScheduledDate).ToList()
            })
            .ToList();

        // Obter lista de meses para apresentar no cabeçalho
        var monthsList = string.Join(", ", sessionsByMonth.Select(m => m.MonthName));

        container.Column(column =>
        {
            // Document Title
            column.Item().PaddingBottom(5).AlignCenter().Text($"Cronograma - {action.Course.Title}")
                .FontSize(14).FontFamily("Arial").Bold();

            // Document Subtitle
            column.Item().PaddingBottom(5).AlignCenter().Text(action.Course.Frame.OperationType)
                .FontSize(10).FontFamily("Arial").Bold();

            // Meses cobertos pelo cronograma
            HelperComposer.AddInfoRow(column, "Meses:", monthsList);

            // Info Rows
            HelperComposer.AddInfoRow(column, "Operação:", action.Course.Frame.Operation);
            HelperComposer.AddInfoRow(column, "Tipologia de Intervenção:", action.Course.Frame.InterventionType);
            HelperComposer.AddInfoRow(column, "Total de Horas:", $"{action.Course.TotalDuration} horas");
            HelperComposer.AddInfoRow(column, "Ação:", action.Title);
            HelperComposer.AddInfoRow(column, "Horários:", action.AllDifferentSessionTimes());
            HelperComposer.AddInfoRow(column, "Regime:", action.Regiment.Humanize());
            HelperComposer.AddInfoRow(column, "Morada:", action.Address);

            // Renderizar uma tabela para cada mês
            foreach (var monthGroup in sessionsByMonth)
            {
                column.Item().PaddingTop(15).Column(monthSection =>
                {
                    // Subtítulo do mês
                    HelperComposer.SubTitle(monthSection, monthGroup.MonthName);

                    // Tabela de sessões do mês
                    monthSection.Item().Table(table =>
                    {
                        ConfigureTableColumns(table);
                        RenderTableHeader(table);

                        foreach (var session in monthGroup.Sessions)
                        {
                            RenderTableRows(table, session);
                        }
                    });
                });
            }
        });
    }

    private static void ConfigureTableColumns(TableDescriptor table)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.RelativeColumn(1); // Data
            columns.RelativeColumn(3); // Módulo / Formador
            columns.RelativeColumn(1); // Horários
            columns.RelativeColumn(1); // Duração
            columns.RelativeColumn(2); // Observação
        });
    }

    private static void RenderTableHeader(TableDescriptor table)
    {
        table.Header(header =>
        {
            header.Cell().Element(HelperComposer.CellStyle).AlignCenter().AlignMiddle().Text("Data").FontSize(8).FontFamily("Arial").SemiBold();
            header.Cell().Element(HelperComposer.CellStyle).AlignCenter().AlignMiddle().Text("Módulo / Formador").FontSize(8).FontFamily("Arial").SemiBold();
            header.Cell().Element(HelperComposer.CellStyle).AlignCenter().AlignMiddle().Text("Horário").FontSize(8).FontFamily("Arial").SemiBold();
            header.Cell().Element(HelperComposer.CellStyle).AlignCenter().AlignMiddle().Text("Duração").FontSize(8).FontFamily("Arial").SemiBold();
            header.Cell().Element(HelperComposer.CellStyle).AlignCenter().AlignMiddle().Text("Observação").FontSize(8).FontFamily("Arial").SemiBold();
        });
    }

    private static void RenderTableRows(TableDescriptor table, Session session)
    {
        table.Cell().Element(HelperComposer.CellStyle).Text(session.ScheduledDate.ToString("dd/MM/yy")).FontSize(7).FontFamily("Arial");
        table.Cell().Element(HelperComposer.CellStyle).Column(column =>
        {
            column.Item().Text(session.ModuleTeaching.Module?.Name ?? "N/A").FontSize(7).FontFamily("Arial");
            var teacherName = session.ModuleTeaching.Teacher?.Person?.FullName ?? 
                $"{session.ModuleTeaching.Teacher?.Person?.FirstName} {session.ModuleTeaching.Teacher?.Person?.LastName}".Trim();
            if (!string.IsNullOrEmpty(teacherName))
            {
                column.Item().Text(teacherName).FontSize(6).FontFamily("Arial");
            }
        });
        table.Cell().Element(HelperComposer.CellStyle).Text(session.Time).FontSize(7).FontFamily("Arial");
        table.Cell().Element(HelperComposer.CellStyle).Text($"{session.DurationHours:F1}h").FontSize(7).FontFamily("Arial");
        table.Cell().Element(HelperComposer.CellStyle).Text(session.Note).FontSize(7).FontFamily("Arial");
    }
}