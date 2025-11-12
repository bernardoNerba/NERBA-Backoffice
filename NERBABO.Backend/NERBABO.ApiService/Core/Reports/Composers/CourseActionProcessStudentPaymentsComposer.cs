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

            // Entidade Formadora
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(40).Text("Entidade Formador:").FontSize(8).FontFamily("Arial");
                row.RelativeItem().BorderBottom(1).Padding(2).Text(infos.Designation ?? "").FontSize(8).FontFamily("Arial");
            });
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
