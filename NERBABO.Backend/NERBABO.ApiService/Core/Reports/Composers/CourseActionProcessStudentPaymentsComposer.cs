using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Shared.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class CourseActionProcessStudentPaymentsComposer(IImageService imageService)
{
    private readonly IImageService _imageService = imageService;

    public Document Compose(CourseAction action, GeneralInfo infos)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);

                page.Header().Element(container => ComposeHeader(container, action, infos));
                page.Content().Padding(5).Element(container => ComposeContent(container, action, infos));
                page.Footer().Element(container => ComposeFooter(container, action, infos));
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
                // left side - general info logo
                if (!string.IsNullOrEmpty(infos.Logo))
                {
                    row.ConstantItem(80).Element(logoContainer =>
                    {
                        try
                        {
                            var generalLogoBytes = _imageService.GetImageAsync(infos.Logo).ConfigureAwait(false).GetAwaiter().GetResult();
                            if (generalLogoBytes is not null)
                            {
                                logoContainer.Image(generalLogoBytes).FitArea();
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
                        row.ConstantItem(80).Element(logoContainer =>
                        {
                            try
                            {
                                var programImageBytes = _imageService.GetImageAsync(action.Course.Frame.ProgramLogo)
                                    .ConfigureAwait(false)
                                    .GetAwaiter()
                                    .GetResult();
                                if (programImageBytes is not null)
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
        container.Column(column =>
        {
            // Document Title
            column.Item().PaddingBottom(10).AlignCenter().Text("Processamento de Pagamentos dos Formandos")
                .FontSize(14).FontFamily("Arial").Bold();

            // Entidade Formadora:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Entidade Formador:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(infos.Designation ?? "").FontSize(8).FontFamily("Arial");
            });

            // Intervenção:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Intervenção:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(action.Course.Frame.Intervention ?? "").FontSize(8).FontFamily("Arial");
            });

            // Operação n.º:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Operação n.º:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(action.Course.Frame.Operation ?? "").FontSize(8).FontFamily("Arial");
            });

            // - UFCD:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("UFCDs:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(action.Course.FormattedModuleNamesJoined ?? "").FontSize(8).FontFamily("Arial");
            });

            // - Curso:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Curso:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(action.Course.Title ?? "").FontSize(8).FontFamily("Arial");
            });

            // - Ação:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Ação:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(action.Title ?? "").FontSize(8).FontFamily("Arial");
            });

            var categories = action.Course.Modules
                .SelectMany(m => m.Categories)
                .Select(c => c.ShortenName)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Table
            column.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Nome (mais largo)
                    foreach (var _ in categories)
                        columns.RelativeColumn(1); // Cada categoria
                    columns.RelativeColumn(1); // Total
                    columns.RelativeColumn(1); // Nº de Dias
                });

                table.Header(header =>
                {
                    // Primeira linha do header
                    header.Cell().RowSpan(2).Element(CellStyle)
                        .AlignMiddle().Text("Nome do Formando")
                        .FontSize(8).SemiBold();

                    header.Cell().ColumnSpan((uint)(categories.Count + 1)).Element(CellStyle)
                        .AlignCenter().Text("Horas / Presenças")
                        .FontSize(8).SemiBold();

                    header.Cell().RowSpan(2).Element(CellStyle)
                        .AlignMiddle().Text("Nº de Dias")
                        .FontSize(8).SemiBold();

                    // Segunda linha do header (sub-headers)
                    foreach (var category in categories)
                    {
                        header.Cell().Element(CellStyle)
                            .AlignCenter().Text(category)
                            .FontSize(8).SemiBold();
                    }

                    header.Cell().Element(CellStyle)
                        .AlignCenter().Text("Total")
                        .FontSize(8).SemiBold();
                });

                // Dados dos alunos
                foreach (var student in action.ActionEnrollments.Select(ae => ae.Student))
                {
                    table.Cell().Element(CellStyle).Text(student.Person.FullName).FontSize(8);

                    foreach (var category in categories)
                    {
                        var hours = 0;
                        table.Cell().Element(CellStyle)
                            .AlignCenter().Text($"{hours:0.00}")
                            .FontSize(8);
                    }

                    var total = 0;
                    table.Cell().Element(CellStyle)
                        .AlignCenter().Text($"{total:0.00}")
                        .FontSize(8);

                    table.Cell().Element(CellStyle)
                        .AlignCenter().Text("0")  
                        .FontSize(8);
                }
                
            });
            


            static IContainer CellStyle(IContainer container) =>
                            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
        });
    }

    private void ComposeFooter(IContainer container, CourseAction action, GeneralInfo infos)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(10).AlignCenter().Text($"{infos.Slug} é Entidade Certificada pela DGERT, C61")
                .FontSize(7).FontFamily("Arial").Italic();
        });
    }
}
