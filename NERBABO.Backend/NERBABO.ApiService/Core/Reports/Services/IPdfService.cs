using NERBABO.ApiService.Core.Reports.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Reports.Services;

/// <summary>
/// Service for generating, managing, and storing PDF reports.
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a session timeline report PDF for the specified action.
    /// </summary>
    /// <param name="actionId">The action ID to generate the report for.</param>
    /// <param name="userId">The ID of the user generating the report.</param>
    /// <returns>A result containing the PDF content as byte array.</returns>
    Task<Result<byte[]>> GenerateSessionReportAsync(long actionId, string userId);

    /// <summary>
    /// Generates a cover action report PDF for the specified action.
    /// </summary>
    /// <param name="actionId">The action ID to generate the report for.</param>
    /// <param name="userId">The ID of the user generating the report.</param>
    /// <returns>A result containing the PDF content as byte array.</returns>
    Task<Result<byte[]>> GenerateCoverReportAsync(long actionId, string userId);

    /// <summary>
    /// Generates a teacher form (Ficha de Formador) PDF for a specific teacher in an action.
    /// </summary>
    /// <param name="actionId">The action ID to generate the form for.</param>
    /// <param name="teacherId">The teacher ID to generate the form for.</param>
    /// <param name="userId">The ID of the user generating the form.</param>
    /// <returns>A result containing the PDF content as byte array.</returns>
    Task<Result<byte[]>> GenerateTeacherFormAsync(long actionId, long teacherId, string userId);

    /// <summary>
    /// Generates a training financing form (SIF FM Template) PDF for a specific action.
    /// </summary>
    /// <param name="actionId">The action ID to generate the form for.</param>
    /// <param name="userId">The ID of the user generating the form.</param>
    /// <returns>A result containing the PDF content as byte array.</returns>
    Task<Result<byte[]>> GenerateTrainingFinancingFormAsync(long actionId, string userId);

    /// <summary>
    /// Retrieves a saved PDF record by type and reference ID.
    /// </summary>
    /// <param name="pdfType">The type of PDF (e.g., SessionReport, SessionDetail, ActionSummary).</param>
    /// <param name="referenceId">The reference ID (ActionId or SessionId).</param>
    /// <returns>A result containing the saved PDF record if found.</returns>
    Task<Result<SavedPdf>> GetSavedPdfAsync(string pdfType, long referenceId);

    /// <summary>
    /// Gets the binary content of a saved PDF file.
    /// </summary>
    /// <param name="savedPdfId">The ID of the saved PDF record.</param>
    /// <returns>A result containing the PDF file content as byte array.</returns>
    Task<Result<byte[]>> GetSavedPdfContentAsync(long savedPdfId);

    /// <summary>
    /// Deletes a saved PDF and its associated file from storage.
    /// </summary>
    /// <param name="savedPdfId">The ID of the saved PDF to delete.</param>
    /// <returns>A result indicating success or failure of the deletion.</returns>
    Task<Result> DeleteSavedPdfAsync(long savedPdfId);

    /// <summary>
    /// Saves PDF content to storage and creates a database record.
    /// Updates existing PDF if content has changed.
    /// </summary>
    /// <param name="pdfType">The type of PDF being saved.</param>
    /// <param name="referenceId">The reference ID for the PDF.</param>
    /// <param name="pdfContent">The PDF content as byte array.</param>
    /// <param name="userId">The ID of the user saving the PDF.</param>
    /// <returns>A result containing the saved PDF record.</returns>
    Task<Result<SavedPdf>> SavePdfAsync(string pdfType, long referenceId, byte[] pdfContent, string userId);
}