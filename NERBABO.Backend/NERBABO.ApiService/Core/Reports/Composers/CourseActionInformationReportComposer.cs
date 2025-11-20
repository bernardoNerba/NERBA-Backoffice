using Humanizer;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.Reports.Composers.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class CourseActionInformationReportComposer(HelperComposer helperComposer)
{
    private readonly HelperComposer _helperComposer = helperComposer;

    public async Task<Document> ComposeAsync(CourseAction action, GeneralInfo infos)
    {
        // Pre-load imgaes asynchronously
        var (generalLogoBytes, programLogoBytes, financementLogoBytes) = await _helperComposer
            .LoadLogosAsync(infos.Logo, action.Course.Frame.ProgramLogo, action.Course.Frame.FinancementLogo);


        // Generate Course Action Information Report PDF
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);

                page.Header().Element(container => HelperComposer.ComposeHeader(container, generalLogoBytes, programLogoBytes));
                page.Content().PaddingTop(5).Element(container => ComposeContent(container, action, infos));
                page.Footer().Element(container => HelperComposer.ComposeFooter(container, financementLogoBytes, $"{infos.Slug} é Entidade Certificada pela DGERT, C61"));
            });
        });
    }

    private void ComposeContent(IContainer container, CourseAction action, GeneralInfo infos)
    {
        // Count unique session dates
        var uniqueSessionDates = action.ModuleTeachings
            .SelectMany(mt => mt.Sessions.Select(s => s.ScheduledDate))
            .Distinct()
            .Count();

        container.Column(column =>
        {
            // Title
            HelperComposer.Title(column, "Informação Geral da Formação");

            // ENTIDADE PROMOTORA Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                HelperComposer.SectionTitle(section, "Entidade Promotora:");

                HelperComposer.AddFormField(section, "Desginação", infos.Designation ?? "", 70);
                HelperComposer.AddFormField(section, "Sede", infos.Site ?? "", 70);
                HelperComposer.AddMultFormField(column,
                [
                    new() { Label = "NIPC", Value = infos.Nipc ?? "", Space = 70},
                    new() { Label = "Entidade Bancária", Value = infos.BankEntity ?? "", Space = 70}
                ]);
            });

            // ENTIDADE FORMADORA Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                HelperComposer.SectionTitle(section, "Entidade Formadora:");
                HelperComposer.AddFormField(section, "Designação", infos.Designation ?? "");
                HelperComposer.AddFormField(section, "Sede", action.Address ?? "");
            });

            // ENQUADRAMENTO Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                HelperComposer.SectionTitle(section, "Enquadramento:");
                HelperComposer.AddFormField(section, "Intervenção", action.Course.Frame.Intervention ?? "");
                HelperComposer.AddFormField(section, "Programa", action.Course.Frame.Program ?? "");
                HelperComposer.AddFormField(section, "Tipologia de Intervenção", action.Course.Frame.InterventionType ?? "");
                HelperComposer.AddFormField(section, "Operação n.º", action.Course.Frame.Operation ?? "");
                HelperComposer.AddFormField(section, "Tipologia de Operação", action.Course.Frame.OperationType ?? "");
            });

            // PEDIDO DE FINANCIAMENTO Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                HelperComposer.SectionTitle(section, "Pedido de Financiamento:");
                HelperComposer.AddFormField(section, "Apólice de Seguro N.º", infos.InsurancePolicy ?? "");
                HelperComposer.AddFormField(section, "UFC/Percurso/Curso", action.Course.Title ?? "");
                HelperComposer.AddFormField(section, "Ação", action.Title ?? "");
                HelperComposer.AddFormField(section, "Código Administrativo", action.AdministrationCode ?? "");

                // Data Inicio, Data Fim, Horário
                HelperComposer.AddMultFormField(column,
                [
                    new() { Label = "Data Inicio", Value = action.StartDate.ToString("dd/MM/yyyy") ?? "", Space = 50},
                    new() { Label = "Data Fim", Value = action.EndDate.ToString("dd/MM/yyyy") ?? "", Space = 40},
                ]);

                // Área, Ano Escolar
                HelperComposer.AddFormField(section, "Área", action.Course.Area ?? "");
                
                // Ano Escolar, Carga horária, Nº. de dias
                HelperComposer.AddMultFormField(column,
                [
                    new() { Label = "Ano Escolar", Value = action.StartDate.Year.ToString() ?? "", Space = 55},
                    new() { Label = "Carga horária", Value = $"{action.Course.TotalDuration}h" ?? "", Space = 55},
                    new() { Label = "Nº. de dias", Value = uniqueSessionDates.ToString() ?? "", Space = 55},
                    new() { Label = "Subsídio alimentação", Value = $"{infos.HourValueAlimentation} €" ?? "", Space = 65},
                ]);

                HelperComposer.AddFormField(section, "Local da Formação", action.Address ?? "");
                HelperComposer.AddFormField(section, "Dias da Semana", action.FormattedWeekDays);
                HelperComposer.AddFormField(section, "Horários", action.AllDifferentSessionTimes() ?? "");
                HelperComposer.AddFormField(section, "Caracterização das instalações", infos.FacilitiesCharacterization ?? "");
            });

            // MÓDULOS Table Section
            column.Item().PaddingTop(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("MÓDULOS:")
                    .FontSize(9).FontFamily("Arial").Bold();

                // Table
                section.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40); // Nº
                        columns.RelativeColumn(); // Designação
                        columns.ConstantColumn(60); // Horas
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("Nº").FontSize(8).FontFamily("Arial").Bold();
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("Designação").FontSize(8).FontFamily("Arial").Bold();
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("Horas").FontSize(8).FontFamily("Arial").Bold();
                    });

                    // Rows - iterate through modules
                    int moduleNumber = 1;
                    foreach (var module in action.Course.Modules)
                    {
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(moduleNumber.ToString()).FontSize(8).FontFamily("Arial");
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text(module.Name ?? "").FontSize(8).FontFamily("Arial");
                        table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(3).Text($"{module.Hours}h").FontSize(8).FontFamily("Arial");
                        moduleNumber++;
                    }

                    // Total row
                    table.Cell().BorderTop(1).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("").FontSize(8).FontFamily("Arial");
                    table.Cell().BorderTop(1).BorderColor(Colors.Grey.Lighten1).Padding(3).Text("TOTAL").FontSize(8).FontFamily("Arial").Bold();
                    table.Cell().BorderTop(1).BorderColor(Colors.Grey.Lighten1).Padding(3).Text($"{action.Course.TotalDuration}h").FontSize(8).FontFamily("Arial").Bold();
                });
            });
        });
    }

}
