using NERBABO.ApiService.Core.Reports.Models;

namespace NERBABO.ApiService.Core.Reports.Services;

public interface IPdfService
{
    Task<byte[]> GenerateSessionReportAsync(long actionId, string userId);
    Task<byte[]> GenerateSessionDetailAsync(long sessionId, string userId);
    Task<byte[]> GenerateActionSummaryAsync(long actionId, string userId);
    
    // Storage-related methods
    Task<SavedPdf?> GetSavedPdfAsync(string pdfType, long referenceId);
    Task<byte[]?> GetSavedPdfContentAsync(long savedPdfId);
    Task<bool> DeleteSavedPdfAsync(long savedPdfId);
    Task<SavedPdf> SavePdfAsync(string pdfType, long referenceId, byte[] pdfContent, string userId, bool replaceExisting = false);
}