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
                page.Content().Element(container => container.Column(column =>
                {
                    // Action details section
                    column.Item().PaddingTop(5).Column(details =>
                    {
                        details.Item().PaddingBottom(5).Text($"Operação: {action.Course.Frame.Operation}")
                            .FontSize(8).FontFamily("Arial");
                        details.Item().PaddingBottom(5).Text($"Tipologia de intervenção: {action.Course.Frame.InterventionType}")
                            .FontSize(8).FontFamily("Arial");
                        details.Item().PaddingBottom(5).Text($"Total de Horas: {action.Course.TotalDuration}")
                            .FontSize(8).FontFamily("Arial");
                        details.Item().PaddingBottom(5).Text($"Ação: {action.Title}")
                            .FontSize(8).FontFamily("Arial");
                        details.Item().PaddingBottom(5).Text($"Horário: {action.AllDifferentSessionTimes()}")
                            .FontSize(8).FontFamily("Arial");
                        if (!string.IsNullOrEmpty(action.Address))
                        {
                        details.Item().PaddingBottom(5).Text($"Morada do Local de realização: {action.Address}")
                            .FontSize(8).FontFamily("Arial");
                        }
                        details.Item().PaddingBottom(5).Text($"Regime: {action.Regiment.Humanize()}")
                            .FontSize(8).FontFamily("Arial");
                    });

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
                }));
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

                // Title row
                column.Item().PaddingBottom(5).Row(row =>
                {
                    // Left side - Title and info
                    row.RelativeItem().AlignCenter().Column(titleColumn =>
                    {
                        titleColumn.Item().AlignLeft().Text($"Cronograma - {action.Course.Title}")
                            .FontSize(14).FontFamily("Arial").SemiBold();
                        titleColumn.Item().AlignLeft().Text(action.Course.Frame.OperationType)
                            .FontSize(10).FontFamily("Arial");
                    });

                    // Right side - Program logo (if available)
                    if (!string.IsNullOrEmpty(action.Course.Frame.ProgramLogo))
                    {
                        row.ConstantItem(50).Element(logoContainer =>
                        {
                            try
                            {
                                var programImageBytes = _imageService.GetImageAsync(action.Course.Frame.ProgramLogo).ConfigureAwait(false).GetAwaiter().GetResult();
                                if (programImageBytes != null)
                                {
                                    logoContainer.Height(50).AlignCenter().AlignMiddle()
                                        .Image(programImageBytes).FitArea();
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log the exception if needed and continue without the image
                                System.Diagnostics.Debug.WriteLine($"Failed to load program logo: {ex.Message}");
                            }
                        });
                    }
                });
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