using NERBABO.ApiService.Core.Reports.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Reports.Services;

public interface IPdfService
{
    Task<Result<byte[]>> GenerateSessionReportAsync(long actionId, string userId);

    // Storage-related methods
    Task<Result<SavedPdf>> GetSavedPdfAsync(string pdfType, long referenceId);
    Task<Result<byte[]>> GetSavedPdfContentAsync(long savedPdfId);
    Task<Result> DeleteSavedPdfAsync(long savedPdfId);
    Task<Result<SavedPdf>> SavePdfAsync(string pdfType, long referenceId, byte[] pdfContent, string userId);
}