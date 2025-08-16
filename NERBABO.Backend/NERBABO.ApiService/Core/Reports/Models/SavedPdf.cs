using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Reports.Models;

public class SavedPdf : Entity<long>
{
    public string PdfType { get; set; } = string.Empty; // SessionReport, SessionDetail, ActionSummary
    public long ReferenceId { get; set; } // ActionId or SessionId
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string ContentHash { get; set; } = string.Empty; // SHA256 hash of content for change detection
    public DateTime GeneratedAt { get; set; }
    public string GeneratedByUserId { get; set; } = string.Empty;
    

}

public static class PdfTypes
{
    public const string SessionReport = "SessionReport";
    public const string SessionDetail = "SessionDetail"; 
    public const string ActionSummary = "ActionSummary";
}