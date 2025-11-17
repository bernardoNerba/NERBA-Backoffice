using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Shared.Enums;
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

    private static void ComposeContent(IContainer container, CourseAction action, GeneralInfo infos)
    {
        container.Column(column =>
        {
            // Document Title
            column.Item().PaddingBottom(20).AlignCenter().Text("Processamento de Pagamentos dos Formandos")
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

            // - Subsídio de Alimentação / hora:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Sub. Alimentação / h:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(infos.HourlySubsidy).FontSize(8).FontFamily("Arial");
            });

            var categories = action.Course.Modules
                .Select(m => m.Category.ShortenName)
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
                    columns.RelativeColumn(1); // Total Pagamento
                });

                table.Header(header =>
                {
                    // Primeira linha do header
                    header.Cell().RowSpan(2).Element(CellStyle)
                        .AlignCenter().AlignMiddle().Text("Formando (IBAN)")
                        .FontSize(8).SemiBold();

                    header.Cell().ColumnSpan((uint)(categories.Count + 1)).Element(CellStyle)
                        .AlignCenter().Text("Horas / Presenças")
                        .FontSize(8).SemiBold();

                    header.Cell().RowSpan(2).Element(CellStyle)
                        .AlignCenter().AlignMiddle().Text("Nº de Dias")
                        .FontSize(8).SemiBold();
                    
                    header.Cell().RowSpan(2).Element(CellStyle)
                        .AlignCenter().AlignMiddle().Text("Total liq.")
                        .FontSize(8).SemiBold();

                    // Segunda linha do header (sub-headers)
                    foreach (var category in categories)
                    {
                        header.Cell().Element(CellStyle)
                            .AlignCenter().AlignMiddle().Text(category)
                            .FontSize(8).SemiBold();
                    }

                    header.Cell().Element(CellStyle)
                        .AlignCenter().AlignMiddle().Text("Total")
                        .FontSize(8).SemiBold();
                    
                });

                // Variáveis para calcular os totais gerais
                var totalCategoryHours = categories.ToDictionary(c => c, c => 0f);
                var grandTotalHours = 0f;
                var grandTotalDays = 0;
                var grandTotalPayment = 0f;

                foreach (var enrollment in action.ActionEnrollments)
                {
                    var hoursByCategory = new Dictionary<string, float>();
        
                    foreach (var participation in enrollment.Participations.Where(p => p.Presence == PresenceEnum.Present))
                    {
                        var module = participation.Session?.ModuleTeaching?.Module;
                        if (module != null)
                        {
                            var categoryName = module.Category.ShortenName;
                            var hours = (float)participation.Attendance;
                            
                            if (hoursByCategory.ContainsKey(categoryName))
                            {
                                hoursByCategory[categoryName] += hours;
                            }
                            else
                            {
                                hoursByCategory[categoryName] = hours;
                            }
                        
                        }
                    }

                    var totalHours = (float)enrollment.Participations
                        .Where(p => p.Presence == PresenceEnum.Present)
                        .Sum(p => p.Attendance);
                    
                    var totalDays = enrollment.Participations
                        .Count(p => p.Presence == PresenceEnum.Present);
                    
                    var totalPayment = enrollment.CalculatedTotal(infos.HourValueAlimentation);

                    // Atualiza os totais gerais
                    foreach (var category in categories)
                    {
                        var hours = hoursByCategory.GetValueOrDefault(category, 0);
                        totalCategoryHours[category] += hours;
                    }
                    grandTotalHours += totalHours;
                    grandTotalDays += totalDays;
                    grandTotalPayment += (float)Math.Round(totalPayment, 2);

                    // Nome + IBAN
                    table.Cell().Element(CellStyle)
                        .AlignMiddle()
                        .Text($"{enrollment.Student.Person.FullName}\n{enrollment.Student.Person.IBAN}")
                        .FontSize(8);

                    // Horas por categoria
                    foreach (var category in categories)
                    {
                        var hours = hoursByCategory.GetValueOrDefault(category, 0);
                        table.Cell().Element(CellStyle)
                            .AlignCenter().AlignMiddle().Text($"{hours:0.00}")
                            .FontSize(8);
                    }

                    // Total horas
                    table.Cell().Element(CellStyle)
                        .AlignCenter().AlignMiddle().Text($"{totalHours:0.00}")
                        .FontSize(8);

                    // Nº dias
                    table.Cell().Element(CellStyle)
                        .AlignCenter().AlignMiddle().Text($"{totalDays}")
                        .FontSize(8);

                    // Total pagamento
                    table.Cell().Element(CellStyle)
                        .AlignCenter().AlignMiddle().Text($"{totalPayment:0.00}")
                        .FontSize(8);
                }

                // Linha de totais gerais
                table.Cell().ColumnSpan(1).Element(TotalCellStyle)
                    .AlignCenter().AlignMiddle().Text("Total")
                    .FontSize(8).Bold();

                // Totais por categoria
                foreach (var category in categories)
                {
                    table.Cell().Element(TotalCellStyle)
                        .AlignCenter().AlignMiddle().Text($"{totalCategoryHours[category]:0.00}")
                        .FontSize(8).Bold();
                }

                // Total geral de horas
                table.Cell().Element(TotalCellStyle)
                    .AlignCenter().AlignMiddle().Text($"{grandTotalHours:0.00}")
                    .FontSize(8).Bold();

                // Total geral de dias
                table.Cell().Element(TotalCellStyle)
                    .AlignCenter().AlignMiddle().Text($"{grandTotalDays}")
                    .FontSize(8).Bold();

                // Total geral de pagamento
                table.Cell().Element(TotalCellStyle)
                    .AlignCenter().AlignMiddle().Text($"{grandTotalPayment:0.00}")
                    .FontSize(8).Bold();
            });
            
            static IContainer CellStyle(IContainer container) =>
                container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
                
            static IContainer TotalCellStyle(IContainer container) =>
                container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                    .Background(Colors.Grey.Lighten3);
        });
    }

    private static void ComposeFooter(IContainer container, CourseAction action, GeneralInfo infos)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(10).AlignCenter().Text($"{infos.Slug} é Entidade Certificada pela DGERT, C61")
                .FontSize(7).FontFamily("Arial").Italic();
        });
    }
}
