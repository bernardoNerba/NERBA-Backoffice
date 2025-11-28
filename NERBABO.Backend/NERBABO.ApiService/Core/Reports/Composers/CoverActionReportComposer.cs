using System.Runtime.CompilerServices;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Core.Global.Models;
using NERBABO.ApiService.Shared.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers;

public class CoverActionReportComposer(HelperComposer helperComposer)
{
    private readonly HelperComposer _helperComposer = helperComposer;

    public async Task<Document> ComposeAsync(CourseAction action, GeneralInfo infos)
    {
        // Pre-load imgaes asynchronously
        var (generalLogoBytes, programLogoBytes, financementLogoBytes) = await _helperComposer
            .LoadLogosAsync(infos.Logo, action.Course.Frame.ProgramLogo, action.Course.Frame.FinancementLogo);


        // Generate PDF Cover Page for Action
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Element(container => HelperComposer.ComposeHeader(container, generalLogoBytes, programLogoBytes));
                page.Content().PaddingTop(30).Element(container => ComposeContent(container, action));
                page.Footer().Element(container => HelperComposer.ComposeFooter(container, financementLogoBytes, $"{infos.Slug} é Entidade Certificada pela DGERT, C61"));
            });
        });
    }

    private void ComposeContent(IContainer container, CourseAction action)
    {
        container.AlignCenter().MaxWidth(450).Column(mainColumn =>
        {
            // Main title (outside the box)
            HelperComposer.Title(mainColumn, "Dossier Técnico - Pedagógico");

            // Data section wrapped in gray box
            mainColumn.Item()
                .Border(2).BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten4).Padding(25)
                .Column(column =>
                {
                    HelperComposer.AddInfoRowCentered(column, "Programa", action.Course.Frame.Program ?? "");
                    HelperComposer.AddInfoRowCentered(column, "Tipologia de Intervenção", action.Course.Frame.InterventionType ?? "");
                    HelperComposer.AddInfoRowCentered(column, "Nº de Operação", action.Course.Frame.Operation ?? "");
                    HelperComposer.AddInfoRowCentered(column, "Curso", action.Course.Title ?? "");

                    // Modules
                    if (action.Course.Modules != null && action.Course.Modules.Count > 0)
                    {
                        column.Item().PaddingTop(5).PaddingBottom(8).AlignCenter().Column(section =>
                        {
                            section.Item().PaddingBottom(3).Text("Módulos:")
                                .FontSize(10).FontFamily("Arial").Bold();

                            foreach (var module in action.Course.Modules.OrderBy(m => m.Id))
                            {
                                section.Item().PaddingBottom(2).Text($"• {module.Name} ({module.Hours}h)")
                                    .FontSize(10).FontFamily("Arial");
                            }
                        });
                    }

                    HelperComposer.AddInfoRowCentered(column, "Ação", action.Title ?? "");
                    HelperComposer.AddInfoRowCentered(column, "Código de Administrativo", action.AdministrationCode ?? "");
                });
        });
    }
}