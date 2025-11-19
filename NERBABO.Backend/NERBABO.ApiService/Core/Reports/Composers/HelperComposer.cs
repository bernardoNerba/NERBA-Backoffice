using NERBABO.ApiService.Shared.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NERBABO.ApiService.Core.Reports.Composers
{

    public class HelperComposer(IImageService imageService)
    {
        private readonly IImageService _imageService = imageService;

        public static void ComposeHeader(IContainer container, byte[]? generalLogo, byte[]? programLogo)
        {
            container.PaddingVertical(5).Row(row =>
            {
                // Left: General Info Logo
                row.ConstantItem(80).Element(logoContainer =>
                {
                    if (generalLogo is not null)
                    {
                        logoContainer.Image(generalLogo).FitArea();
                    }
                });

                row.RelativeItem();

                // Right: Program Logo
                row.ConstantItem(80).Element(logoContainer =>
                {
                    if (programLogo is not null)
                    {
                        logoContainer.Height(40).AlignRight().AlignMiddle()
                            .Image(programLogo).FitArea();
                    }
                });
            });
        }
        
        public static void ComposeFooter(IContainer container, byte[]? financementLogo, string? smallText = "")
        {
            container.AlignCenter().Column(column =>
            {
                //Financement logo (if available)
                if (financementLogo is not null)
                {
                    column.Item()
                        .Height(60)
                        .Image(financementLogo)
                        .FitArea();
                }

                // Small text under the logo (if available)
                if (!string.IsNullOrEmpty(smallText))
                {
                    column.Item()
                        .PaddingTop(3)
                        .Text(smallText)
                        .FontSize(7).FontFamily("Arial").Italic();
                }
            });
        }

        public async Task<(byte[]? general, byte[]? program, byte[]? financement)> LoadLogosAsync(string? generalLogoPath, string? programLogoPath, string? financementLogoPath = null)
        {
            var generalTask = !string.IsNullOrEmpty(generalLogoPath) 
                ? LoadImageSafeAsync(generalLogoPath) 
                : Task.FromResult<byte[]?>(null);
                
            var programTask = !string.IsNullOrEmpty(programLogoPath) 
                ? LoadImageSafeAsync(programLogoPath) 
                : Task.FromResult<byte[]?>(null);
            
            var financementTask = !string.IsNullOrEmpty(financementLogoPath) 
                ? LoadImageSafeAsync(financementLogoPath) 
                : Task.FromResult<byte[]?>(null);

            return (await generalTask, await programTask, await financementTask);
        }

        public async Task<byte[]?> LoadImageSafeAsync(string path)
        {
            try
            {
                return await _imageService.GetImageAsync(path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load image '{path}': {ex.Message}");
                return null;
            }
        }
    
        public static void AddInfoRow(ColumnDescriptor column, string label, string? value)
        {
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(100).Text(label).FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(value ?? "").FontSize(8).FontFamily("Arial");
            });
        }
    
        public static IContainer CellStyle(IContainer container) =>
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
        
        public static IContainer TotalCellStyle(IContainer container) =>
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                .Background(Colors.Grey.Lighten3);
    }
}