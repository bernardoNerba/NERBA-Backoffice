using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Shared.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class CoverActionReportComposer(IImageService imageService)
{
    private readonly IImageService _imageService = imageService;

    public Document Compose(CourseAction action, GeneralInfo infos)
    {
        // Generate PDF Cover Page for Action
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Element(container => ComposeHeader(container, action, infos));
                page.Content().PaddingTop(30).Element(container => ComposeContent(container, action));
                page.Footer().Element(container => ComposeFooter(container, action));
            });
        });
    }

    private void ComposeHeader(IContainer container, CourseAction action, GeneralInfo infos)
    {
        container.PaddingBottom(15).Row(row =>
        {
            // Left side - General Info logo (if available) - LARGER
            if (!string.IsNullOrEmpty(infos.Logo))
            {
                row.ConstantItem(120).Element(logoContainer =>
                {
                    try
                    {
                        var generalLogoBytes = _imageService.GetImageAsync(infos.Logo).ConfigureAwait(false).GetAwaiter().GetResult();
                        if (generalLogoBytes != null)
                        {
                            logoContainer.Height(70).AlignLeft().AlignTop()
                                .Image(generalLogoBytes).FitArea();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the exception if needed and continue without the image
                        System.Diagnostics.Debug.WriteLine($"Failed to load general info logo: {ex.Message}");
                    }
                });
            }
            else
            {
                row.ConstantItem(120);
            }

            // Center spacer
            row.RelativeItem();

            // Right side - Program logo (if available)
            if (!string.IsNullOrEmpty(action.Course.Frame.ProgramLogo))
            {
                row.ConstantItem(100).Element(logoContainer =>
                {
                    try
                    {
                        var programImageBytes = _imageService.GetImageAsync(action.Course.Frame.ProgramLogo).ConfigureAwait(false).GetAwaiter().GetResult();
                        if (programImageBytes != null)
                        {
                            logoContainer.Height(60).AlignRight().AlignTop()
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
            else
            {
                row.ConstantItem(100);
            }
        });
    }

    private void ComposeContent(IContainer container, CourseAction action)
    {
        container.AlignCenter().MaxWidth(450).Column(mainColumn =>
        {
            // Main title (outside the box)
            mainColumn.Item().PaddingBottom(25).AlignCenter().Text("DOSSIER TÉCNICO - PEDAGÓGICO")
                .FontSize(16).FontFamily("Arial").SemiBold();

            // Data section wrapped in gray box
            mainColumn.Item()
                .Border(2)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten4)
                .Padding(25)
                .Column(column =>
                {
                    // Program
                    if (!string.IsNullOrEmpty(action.Course.Frame.Program))
                    {
                        column.Item().PaddingBottom(12).AlignCenter().Column(section =>
                        {
                            section.Item().PaddingBottom(3).AlignCenter().Text("Programa:")
                                .FontSize(10).FontFamily("Arial").SemiBold();
                            section.Item().AlignCenter().Text(action.Course.Frame.Program)
                                .FontSize(10).FontFamily("Arial");
                        });
                    }

                    // Intervention Type
                    if (!string.IsNullOrEmpty(action.Course.Frame.InterventionType))
                    {
                        column.Item().PaddingBottom(12).AlignCenter().Column(section =>
                        {
                            section.Item().PaddingBottom(3).AlignCenter().Text("Tipologia de Intervenção:")
                                .FontSize(10).FontFamily("Arial").SemiBold();
                            section.Item().AlignCenter().Text(action.Course.Frame.InterventionType)
                                .FontSize(10).FontFamily("Arial");
                        });
                    }
                    
                    // Operation Number
                    if (!string.IsNullOrEmpty(action.Course.Frame.Operation))
                    {
                        column.Item().PaddingBottom(12).AlignCenter().Column(section =>
                        {
                            section.Item().PaddingBottom(3).AlignCenter().Text("Nº de Operação:")
                                .FontSize(10).FontFamily("Arial").SemiBold();
                            section.Item().AlignCenter().Text(action.Course.Frame.Operation)
                                .FontSize(10).FontFamily("Arial");
                        });
                    }

                    // Course Title
                    column.Item().PaddingBottom(12).AlignCenter().Column(section =>
                    {
                        section.Item().PaddingBottom(3).AlignCenter().Text("Curso:")
                            .FontSize(10).FontFamily("Arial").SemiBold();
                        section.Item().AlignCenter().Text(action.Course.Title)
                            .FontSize(10).FontFamily("Arial");
                    });

                    // Modules
                    if (action.Course.Modules != null && action.Course.Modules.Count > 0)
                    {
                        column.Item().PaddingBottom(12).AlignCenter().Column(section =>
                        {
                            section.Item().PaddingBottom(5).AlignCenter().Text("Módulos:")
                                .FontSize(10).FontFamily("Arial").SemiBold();

                            foreach (var module in action.Course.Modules.OrderBy(m => m.Id))
                            {
                                section.Item().PaddingBottom(2).AlignCenter().Text($"• {module.Name} ({module.Hours}h)")
                                    .FontSize(9).FontFamily("Arial");
                            }
                        });
                    }

                    // Action Title
                    column.Item().PaddingBottom(12).AlignCenter().Column(section =>
                    {
                        section.Item().PaddingBottom(3).AlignCenter().Text("Ação:")
                            .FontSize(10).FontFamily("Arial").SemiBold();
                        section.Item().AlignCenter().Text(action.Title)
                            .FontSize(10).FontFamily("Arial");
                    });

                    // Administration Code
                    if (!string.IsNullOrEmpty(action.AdministrationCode))
                    {
                        column.Item().AlignCenter().Column(section =>
                        {
                            section.Item().PaddingBottom(3).AlignCenter().Text("Código de Administrativo:")
                                .FontSize(10).FontFamily("Arial").SemiBold();
                            section.Item().AlignCenter().Text(action.AdministrationCode)
                                .FontSize(10).FontFamily("Arial");
                        });
                    }
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