using Humanizer;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Core.Sessions.Models;
using NERBABO.ApiService.Shared.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class SessionsTimelineComposer(IImageService imageService)
{
    private readonly IImageService _imageService = imageService;

    public Document Compose(List<Session> sessions, CourseAction action, GeneralInfo infos)
    {
        // Generate PDF using existing logic
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Element(container => ComposeHeader(container, action, infos));
                page.Content().Element(container => ComposeContent(container, action, infos, sessions));
                page.Footer().Element(container => ComposeFooter(container, action));
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

    private static void ComposeContent(IContainer container, CourseAction action, GeneralInfo infos, List<Session> sessions)
    {
        container.Column(column =>
        {
            // Document Title
            column.Item().PaddingBottom(5).AlignCenter().Text($"Cronograma - {action.Course.Title}")
                .FontSize(14).FontFamily("Arial").Bold();
            // Document Subtitle
            column.Item().PaddingBottom(20).AlignCenter().Text(action.Course.Frame.OperationType)
                .FontSize(10).FontFamily("Arial").Bold();
            
            // Operação:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Operação:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(action.Course.Frame.Operation ?? "").FontSize(8).FontFamily("Arial");
            });
            
            // Tipologia de Intervenção:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Tipologia de Intervenção:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(action.Course.Frame.InterventionType ?? "").FontSize(8).FontFamily("Arial");
            });

            // Total de Horas:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Total de Horas:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text($"{action.Course.TotalDuration} horas" ?? "").FontSize(8).FontFamily("Arial");
            });

            // Ação:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Ação:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text($"{action.Title}" ?? "").FontSize(8).FontFamily("Arial");
            });

            // Horário:
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text("Horário:").FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text($"{action.AllDifferentSessionTimes()}" ?? "").FontSize(8).FontFamily("Arial");
            });

            // Morada:
            if (!string.IsNullOrEmpty(action.Address))
            {
                column.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(100).Text("Morada:").FontSize(8).FontFamily("Arial").Bold();
                    row.RelativeItem().Text($"{action.Address}" ?? "").FontSize(8).FontFamily("Arial");
                });
            }

            // Regime:
            if (!string.IsNullOrEmpty(action.Address))
            {
                column.Item().PaddingBottom(3).Row(row =>
                {
                    row.ConstantItem(100).Text("Regime:").FontSize(8).FontFamily("Arial").Bold();
                    row.RelativeItem().Text($"{action.Regiment.Humanize()}" ?? "").FontSize(8).FontFamily("Arial");
                });
            }

            // Sessions table section
            column.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Data").FontSize(8).FontFamily("Arial").SemiBold();
                    header.Cell().Element(CellStyle).Text("Módulo / Formador").FontSize(8).FontFamily("Arial").SemiBold();
                    header.Cell().Element(CellStyle).Text("Horário").FontSize(8).FontFamily("Arial").SemiBold();
                    header.Cell().Element(CellStyle).Text("Duração").FontSize(8).FontFamily("Arial").SemiBold();
                    header.Cell().Element(CellStyle).Text("Observação").FontSize(8).FontFamily("Arial").SemiBold();
                });

                foreach (var session in sessions)
                {
                    table.Cell().Element(CellStyle).Text(session.ScheduledDate.ToString("dd/MM/yy")).FontSize(7).FontFamily("Arial");
                    table.Cell().Element(CellStyle).Column(column =>
                    {
                        column.Item().Text(session.ModuleTeaching.Module?.Name ?? "N/A").FontSize(7).FontFamily("Arial");
                        var teacherName = session.ModuleTeaching.Teacher?.Person?.FullName ?? 
                            $"{session.ModuleTeaching.Teacher?.Person?.FirstName} {session.ModuleTeaching.Teacher?.Person?.LastName}".Trim();
                        if (!string.IsNullOrEmpty(teacherName))
                        {
                            column.Item().Text(teacherName).FontSize(6).FontFamily("Arial");
                        }
                    });
                    table.Cell().Element(CellStyle).Text(session.Time).FontSize(7).FontFamily("Arial");
                    table.Cell().Element(CellStyle).Text($"{session.DurationHours:F1}h").FontSize(7).FontFamily("Arial");
                    table.Cell().Element(CellStyle).Text(session.Note).FontSize(7).FontFamily("Arial");
                }

                static IContainer CellStyle(IContainer container) =>
                    container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
            });
        });
    }

    private void ComposeFooter(IContainer container, CourseAction action)
    {
        container.Column(column =>
        {
            // Financement logo at the bottom (if available)
            if (!string.IsNullOrEmpty(action.Course.Frame.FinancementLogo))
            {
                column.Item().AlignCenter().Element(logoContainer =>
                {
                    try
                    {
                        var financementImageBytes = _imageService.GetImageAsync(action.Course.Frame.FinancementLogo).ConfigureAwait(false).GetAwaiter().GetResult();
                        if (financementImageBytes != null)
                        {
                            logoContainer.Height(80).AlignCenter().AlignMiddle()
                                .Image(financementImageBytes).FitArea();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception if needed and continue without the image
                        System.Diagnostics.Debug.WriteLine($"Failed to load financement logo: {ex.Message}");
                    }
                });
            }
        });
    }
}