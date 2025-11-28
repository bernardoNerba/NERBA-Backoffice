using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Enrollments.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.Reports.Dtos;
using NERBABO.ApiService.Shared.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class CourseActionProcessStudentPaymentsComposer(HelperComposer helperComposer)
{
    private readonly HelperComposer _helperComposer = helperComposer;

    public async Task<Document> ComposeAsync(CourseAction action, GeneralInfo infos)
    {
        // Pre-load images asynchronously
        var (generalLogoBytes, programLogoBytes, financementLogoBytes) = await _helperComposer
            .LoadLogosAsync(infos.Logo, action.Course.Frame.ProgramLogo);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);

                page.Header().Element(c => HelperComposer.ComposeHeader(c, generalLogoBytes, programLogoBytes));
                page.Content().Padding(5).Element(c => ComposeContent(c, action, infos));
                page.Footer().Element(c => HelperComposer.ComposeFooter(c, financementLogoBytes, $"{infos.Slug} é Entidade Certificada pela DGERT, C61"));
            });
        });
    }

    private static void ComposeContent(IContainer container, CourseAction action, GeneralInfo infos)
    {
        // Obter todas as datas de sessões e agrupar por mês/ano
        var sessionsByMonth = action.ActionEnrollments
            .SelectMany(e => e.Participations)
            .Where(p => p.Session != null && p.Presence == PresenceEnum.Present)
            .Select(p => p.Session.ScheduledDate)
            .Distinct()
            .OrderBy(d => d)
            .GroupBy(d => new { d.Year, d.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy", new System.Globalization.CultureInfo("pt-PT")),
                FirstDay = new DateOnly(g.Key.Year, g.Key.Month, 1),
                LastDay = new DateOnly(g.Key.Year, g.Key.Month, DateTime.DaysInMonth(g.Key.Year, g.Key.Month))
            })
            .ToList();

        // Obter lista de meses para apresentar no cabeçalho
        var monthsList = string.Join(", ", sessionsByMonth.Select(m => m.MonthName));

        container.Column(column =>
        {
            // Document Title
            column.Item().PaddingBottom(5).AlignCenter()
                .Text("Processamento de Pagamentos dos Formandos")
                .FontSize(14).FontFamily("Arial").Bold();

            // Meses cobertos pelo documento
            HelperComposer.AddInfoRow(column, "Meses:", monthsList);

            // Info Rows
            HelperComposer.AddInfoRow(column, "Entidade Formador:", infos.Designation);
            HelperComposer.AddInfoRow(column, "Intervenção:", action.Course.Frame.Intervention);
            HelperComposer.AddInfoRow(column, "Operação n.º:", action.Course.Frame.Operation);
            HelperComposer.AddInfoRow(column, "UFCDs:", action.Course.FormattedModuleNamesJoined);
            HelperComposer.AddInfoRow(column, "Curso:", action.Course.Title);
            HelperComposer.AddInfoRow(column, "Ação:", action.Title);
            HelperComposer.AddInfoRow(column, "Sub. Alimentação / h:", infos.HourlySubsidy);

            var categories = action.Course.Modules
                .Select(m => m.Category.ShortenName)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Renderizar uma tabela para cada mês
            foreach (var monthGroup in sessionsByMonth)
            {
                column.Item().PaddingTop(15).Column(monthSection =>
                {
                    // Subtítulo do mês
                    HelperComposer.SubTitle(monthSection, monthGroup.MonthName);

                    // Tabela de pagamentos do mês
                    monthSection.Item().Table(table =>
                    {
                        ConfigureTableColumns(table, categories.Count);
                        RenderTableHeader(table, categories);

                        var totals = RenderTableRowsForMonth(table, action, infos, categories,
                            monthGroup.FirstDay, monthGroup.LastDay);

                        RenderTableTotals(table, categories, totals);
                    });
                });
            }
        });
    }

    private static void ConfigureTableColumns(TableDescriptor table, int categoryCount)
    {
        table.ColumnsDefinition(columns =>
        {
            columns.RelativeColumn(2); // Nome
            for (int i = 0; i < categoryCount; i++)
                columns.RelativeColumn(1); // Categorias
            columns.RelativeColumn(1); // Total Horas
            columns.RelativeColumn(1); // Nº Dias
            columns.RelativeColumn(1); // Total Pagamento
        });
    }

    private static void RenderTableHeader(TableDescriptor table, List<string> categories)
    {
        table.Header(header =>
        {
            // Row 1
            header.Cell().RowSpan(2).Element(HelperComposer.CellStyle)
                .AlignCenter().AlignMiddle().Text("Formando (IBAN)")
                .FontSize(8).SemiBold();

            header.Cell().ColumnSpan((uint)(categories.Count + 1)).Element(HelperComposer.CellStyle)
                .AlignCenter().Text("Horas / Presenças")
                .FontSize(8).SemiBold();

            header.Cell().RowSpan(2).Element(HelperComposer.CellStyle)
                .AlignCenter().AlignMiddle().Text("Nº de Dias")
                .FontSize(8).SemiBold();
            
            header.Cell().RowSpan(2).Element(HelperComposer.CellStyle)
                .AlignCenter().AlignMiddle().Text("Total liq.")
                .FontSize(8).SemiBold();

            // Row 2: Categories
            foreach (var category in categories)
            {
                header.Cell().Element(HelperComposer.CellStyle)
                    .AlignCenter().AlignMiddle().Text(category)
                    .FontSize(8).SemiBold();
            }

            header.Cell().Element(HelperComposer.CellStyle)
                .AlignCenter().AlignMiddle().Text("Total")
                .FontSize(8).SemiBold();
        });
    }

    private static PaymentTableTotals RenderTableRows(TableDescriptor table, CourseAction action,
        GeneralInfo infos, List<string> categories)
    {
        var totals = new PaymentTableTotals(categories);

        foreach (var enrollment in action.ActionEnrollments)
        {
            var stats = CalculateEnrollmentStats(enrollment, infos.HourValueAlimentation, categories);
            totals.Add(stats);

            // Student Name + IBAN
            table.Cell().Element(HelperComposer.CellStyle)
                .AlignMiddle()
                .Text($"{enrollment.Student.Person.FullName}\n{enrollment.Student.Person.IBAN}")
                .FontSize(8);

            // Hours by Category
            foreach (var category in categories)
            {
                var hours = stats.HoursByCategory.GetValueOrDefault(category, 0f);
                table.Cell().Element(HelperComposer.CellStyle)
                    .AlignCenter().AlignMiddle().Text($"{hours:0.00}")
                    .FontSize(8);
            }

            // Total Hours
            table.Cell().Element(HelperComposer.CellStyle)
                .AlignCenter().AlignMiddle().Text($"{stats.TotalHours:0.00}")
                .FontSize(8);

            // Total Days
            table.Cell().Element(HelperComposer.CellStyle)
                .AlignCenter().AlignMiddle().Text($"{stats.TotalDays}")
                .FontSize(8);

            // Total Payment
            table.Cell().Element(HelperComposer.CellStyle)
                .AlignCenter().AlignMiddle().Text($"{stats.TotalPayment:0.00}")
                .FontSize(8);
        }

        return totals;
    }

    private static PaymentTableTotals RenderTableRowsForMonth(TableDescriptor table, CourseAction action,
        GeneralInfo infos, List<string> categories, DateOnly firstDay, DateOnly lastDay)
    {
        var totals = new PaymentTableTotals(categories);

        foreach (var enrollment in action.ActionEnrollments)
        {
            var stats = CalculateEnrollmentStatsForMonth(enrollment, infos.HourValueAlimentation, categories, firstDay, lastDay);

            // Apenas renderizar linha se o formando tiver presenças neste mês
            if (stats.TotalDays > 0)
            {
                totals.Add(stats);

                // Student Name + IBAN
                table.Cell().Element(HelperComposer.CellStyle)
                    .AlignMiddle()
                    .Text($"{enrollment.Student.Person.FullName}\n{enrollment.Student.Person.IBAN}")
                    .FontSize(8);

                // Hours by Category
                foreach (var category in categories)
                {
                    var hours = stats.HoursByCategory.GetValueOrDefault(category, 0f);
                    table.Cell().Element(HelperComposer.CellStyle)
                        .AlignCenter().AlignMiddle().Text($"{hours:0.00}")
                        .FontSize(8);
                }

                // Total Hours
                table.Cell().Element(HelperComposer.CellStyle)
                    .AlignCenter().AlignMiddle().Text($"{stats.TotalHours:0.00}")
                    .FontSize(8);

                // Total Days
                table.Cell().Element(HelperComposer.CellStyle)
                    .AlignCenter().AlignMiddle().Text($"{stats.TotalDays}")
                    .FontSize(8);

                // Total Payment
                table.Cell().Element(HelperComposer.CellStyle)
                    .AlignCenter().AlignMiddle().Text($"{stats.TotalPayment:0.00}")
                    .FontSize(8);
            }
        }

        return totals;
    }

    private static EnrollmentStats CalculateEnrollmentStats(
        ActionEnrollment enrollment,
        float hourValue,
        List<string> categories)
    {
        var stats = new EnrollmentStats(categories);

        var presentParticipations = enrollment.Participations
            .Where(p => p.Presence == PresenceEnum.Present)
            .ToList();

        foreach (var participation in presentParticipations)
        {
            var module = participation.Session?.ModuleTeaching?.Module;
            if (module is not null)
            {
                var categoryName = module.Category.ShortenName;
                var hours = (float)participation.Attendance;

                if (stats.HoursByCategory.ContainsKey(categoryName))
                    stats.HoursByCategory[categoryName] += hours;
                else
                    stats.HoursByCategory[categoryName] = hours;
            }
        }

        stats.TotalHours = (float)presentParticipations.Sum(p => p.Attendance);
        stats.TotalDays = presentParticipations.Count;
        stats.TotalPayment = (float)Math.Round(enrollment.CalculatedTotal(hourValue), 2);

        return stats;
    }

    private static EnrollmentStats CalculateEnrollmentStatsForMonth(
        ActionEnrollment enrollment,
        float hourValue,
        List<string> categories,
        DateOnly firstDay,
        DateOnly lastDay)
    {
        var stats = new EnrollmentStats(categories);

        var presentParticipations = enrollment.Participations
            .Where(p => p.Presence == PresenceEnum.Present &&
                       p.Session != null &&
                       p.Session.ScheduledDate >= firstDay &&
                       p.Session.ScheduledDate <= lastDay)
            .ToList();

        foreach (var participation in presentParticipations)
        {
            var module = participation.Session?.ModuleTeaching?.Module;
            if (module is not null)
            {
                var categoryName = module.Category.ShortenName;
                var hours = (float)participation.Attendance;

                if (stats.HoursByCategory.ContainsKey(categoryName))
                    stats.HoursByCategory[categoryName] += hours;
                else
                    stats.HoursByCategory[categoryName] = hours;
            }
        }

        stats.TotalHours = (float)presentParticipations.Sum(p => p.Attendance);
        stats.TotalDays = presentParticipations.Count;
        stats.TotalPayment = stats.TotalHours * hourValue;

        return stats;
    }

    private static void RenderTableTotals(TableDescriptor table, List<string> categories, PaymentTableTotals totals)
    {
        table.Cell().Element(HelperComposer.TotalCellStyle)
            .AlignCenter().AlignMiddle().Text("Total")
            .FontSize(8).Bold();

        foreach (var category in categories)
        {
            table.Cell().Element(HelperComposer.TotalCellStyle)
                .AlignCenter().AlignMiddle().Text($"{totals.CategoryTotals[category]:0.00}")
                .FontSize(8).Bold();
        }

        table.Cell().Element(HelperComposer.TotalCellStyle)
            .AlignCenter().AlignMiddle().Text($"{totals.TotalHours:0.00}")
            .FontSize(8).Bold();

        table.Cell().Element(HelperComposer.TotalCellStyle)
            .AlignCenter().AlignMiddle().Text($"{totals.TotalDays}")
            .FontSize(8).Bold();

        table.Cell().Element(HelperComposer.TotalCellStyle)
            .AlignCenter().AlignMiddle().Text($"{totals.TotalPayment:0.00}")
            .FontSize(8).Bold();
    }
}