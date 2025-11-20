using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NERBABO.ApiService.Core.Reports.Composers.Dtos;
using NERBABO.ApiService.Shared.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZLinq;

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
    
        public static void AddInfoRow(ColumnDescriptor column, string label, string? value, int space = 100)
        {
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(space).Text(label).FontSize(8).FontFamily("Arial").Bold();
                row.RelativeItem().Text(value ?? "").FontSize(8).FontFamily("Arial");
            });
        }
    
        public static void AddFormField(ColumnDescriptor column, string label, string value, int space = 100)
        {
            column.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(space).Text($"{label} :").FontSize(8).FontFamily("Arial");
                row.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                    .Padding(2).Text(value).FontSize(8).FontFamily("Arial");
            });
        }

        public static void AddMultFormField(ColumnDescriptor column, ICollection<FormField> fields)
        {
            column.Item().PaddingBottom(3).Row(row =>
            {
                foreach (var field in fields)
                {
                    row.ConstantItem(field.Space).Text($"{field.Label} :").FontSize(8).FontFamily("Arial");
                    row.RelativeItem().BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                        .Padding(2).Text(field.Value).FontSize(8).FontFamily("Arial");
                }
            });
        }

        public static void Title(ColumnDescriptor column, string title)
        {
            column.Item().PaddingBottom(10).AlignCenter().Text(title)
                .FontSize(14).FontFamily("Arial").Bold();
        }

        public static void SubTitle(ColumnDescriptor column, string subTitle)
        {
            column.Item().PaddingBottom(10).AlignCenter().Text(subTitle)
                .FontSize(12).FontFamily("Arial").Bold();
        }

        public static void SectionTitle(ColumnDescriptor column, string title)
        {
            column.Item().PaddingBottom(5).Text(title.ToUpper())
                .FontSize(10).FontFamily("Arial").Bold();
        }

        public static string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength) + "...";
        }

        public static IContainer CellStyle(IContainer container) =>
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
        
        public static IContainer TotalCellStyle(IContainer container) =>
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6)
                .Background(Colors.Grey.Lighten3);
    }
}