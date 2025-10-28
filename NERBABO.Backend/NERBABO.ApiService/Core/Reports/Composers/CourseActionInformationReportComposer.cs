using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Shared.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class CourseActionInformationReportComposer(IImageService imageService)
{
    private readonly IImageService _imageService = imageService;

    public Document Compose(CourseAction action, GeneralInfo infos)
    {
        // Generate Course Action Information Report PDF
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);

                page.Header().Element(container => ComposeHeader(container, action, infos));
                page.Content().PaddingTop(5).Element(container => ComposeContent(container, action, infos));
                page.Footer().Element(container => ComposeFooter(container, infos));
            });
        });
    }

    private void ComposeHeader(IContainer container, CourseAction action, GeneralInfo infos)
    {
        container.Column(column =>
        {
            // Logos row
            column.Item().PaddingVertical(5).Row(row =>
            {
                // Left side - General Info logo
                if (!string.IsNullOrEmpty(infos.Logo))
                {
                    row.ConstantItem(80).Element(logoContainer =>
                    {
                        try
                        {
                            var generalLogoBytes = _imageService.GetImageAsync(infos.Logo).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (generalLogoBytes != null)
                            {
                                logoContainer.Height(40).AlignLeft().AlignMiddle()
                                    .Image(generalLogoBytes).FitArea();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to load general info logo: {ex.Message}");
                        }
                    });
                }
                else
                {
                    row.ConstantItem(80);
                }

                row.RelativeItem();

                // Right side - Program logo
                if (!string.IsNullOrEmpty(action.Course.Frame.ProgramLogo))
                {
                    row.ConstantItem(80).Element(logoContainer =>
                    {
                        try
                        {
                            var programImageBytes = _imageService.GetImageAsync(action.Course.Frame.ProgramLogo).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (programImageBytes != null)
                            {
                                logoContainer.Height(40).AlignRight().AlignMiddle()
                                    .Image(programImageBytes).FitArea();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to load program logo: {ex.Message}");
                        }
                    });
                }
                else
                {
                    row.ConstantItem(80);
                }
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
            column.Item().PaddingBottom(10).AlignCenter().Text("Informação Geral da Formação")
                .FontSize(12).FontFamily("Arial").Bold();

            // ENTIDADE PROMOTORA Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("ENTIDADE PROMOTORA:")
                    .FontSize(9).FontFamily("Arial").Bold();

                // Nome
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(40).Text("Nome:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(infos.Designation ?? "").FontSize(8).FontFamily("Arial");
                });

                // NIPC
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(40).Text("NIPC:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(150).BorderBottom(1).Padding(2).Text(infos.Nipc ?? "").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(100).PaddingLeft(10).Text("Entidade Bancária:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(infos.BankEntity ?? "").FontSize(8).FontFamily("Arial");
                });

                // Sede
                section.Item().Row(row =>
                {
                    row.ConstantItem(40).Text("Sede:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(infos.Site ?? "").FontSize(8).FontFamily("Arial");
                });
            });

            // ENTIDADE FORMADORA Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("ENTIDADE FORMADORA:")
                    .FontSize(9).FontFamily("Arial").Bold();

                // Nome
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(40).Text("Nome:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(infos.Designation ?? "").FontSize(8).FontFamily("Arial");
                });

                // Sede
                section.Item().Row(row =>
                {
                    row.ConstantItem(40).Text("Sede:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.Address ?? "").FontSize(8).FontFamily("Arial");
                });
            });

            // ENQUADRAMENTO Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("ENQUADRAMENTO:")
                    .FontSize(9).FontFamily("Arial").Bold();

                // Intervenção
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(120).Text("Intervenção:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.Course.Frame.Intervention ?? "").FontSize(8).FontFamily("Arial");
                });

                // Programa
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(120).Text("Programa:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.Course.Frame.Program ?? "").FontSize(8).FontFamily("Arial");
                });

                // Tipologia de Intervenção
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(120).Text("Tipologia de Intervenção:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.Course.Frame.InterventionType ?? "").FontSize(8).FontFamily("Arial");
                });

                // Operação n.º
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(120).Text("Operação n.º:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.Course.Frame.Operation ?? "").FontSize(8).FontFamily("Arial");
                });

                // Tipologia de Operação
                section.Item().Row(row =>
                {
                    row.ConstantItem(120).Text("Tipologia de Operação:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.Course.Frame.OperationType ?? "").FontSize(8).FontFamily("Arial");
                });
            });

            // ENQUADRAMENTO / PEDIDO DE FINANCIAMENTO Section
            column.Item().PaddingBottom(10).Column(section =>
            {
                section.Item().PaddingBottom(5).Text("ENQUADRAMENTO / PEDIDO DE FINANCIAMENTO:")
                    .FontSize(9).FontFamily("Arial").Bold();

                // Apólice de Seguro N.º and Operação n.º
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(110).Text("Apólice de Seguro N.º:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(150).BorderBottom(1).Padding(2).Text(infos.InsurancePolicy ?? "").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(80).PaddingLeft(10).Text("Operação n.º:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.Course.Frame.Operation ?? "").FontSize(8).FontFamily("Arial");
                });

                // UFC/Percurso/Curso
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(110).Text("UFC/Percurso/Curso:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.Course.Title ?? "").FontSize(8).FontFamily("Arial");
                });

                // Ação and Código Administrativo
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(40).Text("Ação:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(200).BorderBottom(1).Padding(2).Text(action.Title ?? "").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(130).PaddingLeft(10).Text("Código Administrativo de SIGO:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.AdministrationCode ?? "").FontSize(8).FontFamily("Arial");
                });

                // Data Inicio, Data Fim, Horário
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(65).Text("Data Inicio:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(80).BorderBottom(1).Padding(2).Text(action.StartDate.ToString("dd/MM/yyyy")).FontSize(8).FontFamily("Arial");
                    row.ConstantItem(60).PaddingLeft(10).Text("Data Fim:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(80).BorderBottom(1).Padding(2).Text(action.EndDate.ToString("dd/MM/yyyy")).FontSize(8).FontFamily("Arial");
                    row.ConstantItem(50).PaddingLeft(10).Text("Horário:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.AllDiferentSessionTimes() ?? "").FontSize(8).FontFamily("Arial");
                });

                // Área, Ano Escolar
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(40).Text("Área:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(200).BorderBottom(1).Padding(2).Text(action.Course.Area ?? "").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(80).PaddingLeft(10).Text("Ano Escolar:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.StartDate.Year.ToString()).FontSize(8).FontFamily("Arial");
                });

                // Carga horária, Nº. de dias
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(85).Text("Carga horária:").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(80).BorderBottom(1).Padding(2).Text($"{action.Course.TotalDuration}h").FontSize(8).FontFamily("Arial");
                    row.ConstantItem(70).PaddingLeft(10).Text("Nº. de dias:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(uniqueSessionDates.ToString()).FontSize(8).FontFamily("Arial");
                });

                // Morada/Local da Formação
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(140).Text("Morada/Local da Formação:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(action.Address ?? "").FontSize(8).FontFamily("Arial");
                });

                // Dias da Semana
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(100).Text("Dias da Semana:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(string.Join(", ", action.WeekDays)).FontSize(8).FontFamily("Arial");
                });

                // Caracterização das instalações
                section.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(160).Text("Caracterização das instalações:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text(infos.FacilitiesCharacterization ?? "").FontSize(8).FontFamily("Arial");
                });

                // Subsídio alimentação / dia
                section.Item().Row(row =>
                {
                    row.ConstantItem(140).Text("Subsídio alimentação / dia:").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).Padding(2).Text($"€{infos.HourValueAlimentation}").FontSize(8).FontFamily("Arial");
                });
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
                        header.Cell().BorderBottom(1).Padding(3).Text("Nº").FontSize(8).FontFamily("Arial").Bold();
                        header.Cell().BorderBottom(1).Padding(3).Text("Designação").FontSize(8).FontFamily("Arial").Bold();
                        header.Cell().BorderBottom(1).Padding(3).Text("Horas").FontSize(8).FontFamily("Arial").Bold();
                    });

                    // Rows - iterate through modules
                    int moduleNumber = 1;
                    foreach (var module in action.Course.Modules)
                    {
                        table.Cell().BorderBottom(0.5f).Padding(3).Text(moduleNumber.ToString()).FontSize(8).FontFamily("Arial");
                        table.Cell().BorderBottom(0.5f).Padding(3).Text(module.Name ?? "").FontSize(8).FontFamily("Arial");
                        table.Cell().BorderBottom(0.5f).Padding(3).Text($"{module.Hours}h").FontSize(8).FontFamily("Arial");
                        moduleNumber++;
                    }

                    // Total row
                    table.Cell().BorderTop(1).Padding(3).Text("").FontSize(8).FontFamily("Arial");
                    table.Cell().BorderTop(1).Padding(3).Text("TOTAL").FontSize(8).FontFamily("Arial").Bold();
                    table.Cell().BorderTop(1).Padding(3).Text($"{action.Course.TotalDuration}h").FontSize(8).FontFamily("Arial").Bold();
                });
            });
        });
    }

    private void ComposeFooter(IContainer container, GeneralInfo infos)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(10).AlignCenter().Text($"{infos.Slug} é Entidade Certificada pela DGERT, C61")
                .FontSize(7).FontFamily("Arial").Italic();
        });
    }
}
