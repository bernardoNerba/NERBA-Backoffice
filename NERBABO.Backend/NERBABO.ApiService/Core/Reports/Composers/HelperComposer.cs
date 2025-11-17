using NERBABO.ApiService.Core.Reports.Models;
using NERBABO.ApiService.Shared.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace NERBABO.ApiService.Core.Reports.Composers
{

    public class HelperComposer(IImageService imageService)
    {
        private readonly IImageService _imageService = imageService;

        public async Task<(byte[]? general, byte[]? program)> LoadLogosAsync(string? generalLogoPath, string? programLogoPath)
        {
            var generalTask = !string.IsNullOrEmpty(generalLogoPath) 
                ? LoadImageSafeAsync(generalLogoPath) 
                : Task.FromResult<byte[]?>(null);
                
            var programTask = !string.IsNullOrEmpty(programLogoPath) 
                ? LoadImageSafeAsync(programLogoPath) 
                : Task.FromResult<byte[]?>(null);

            return (await generalTask, await programTask);
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