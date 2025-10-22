using System.Security.Cryptography;
using System.Text;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Reports.Models;

public class SavedPdf : Entity<long>
{
    public string PdfType { get; set; } = string.Empty; // SessionReport, SessionDetail, ActionSummary
    public long ReferenceId { get; set; } // ActionId or SessionId
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// SHA256 hash of content for change detection
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string GeneratedByUserId { get; set; } = string.Empty;


    public static string GenerateFileName(string pdfType, long referenceId)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return $"{pdfType}_{referenceId}_{timestamp}.pdf";
    }
    
    public static string ComputeSha256Hash(byte[] data)
    {
        using SHA256 sha256Hash = SHA256.Create();
        
        byte[] bytes = sha256Hash.ComputeHash(data);
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }
        return builder.ToString();
    }

}

public static class PdfTypes
{
    // Generation Related
    public const string CoverActionReport = "CoverActionReport";
    public const string SessionReport = "SessionReport";
    public const string TeacherForm = "TeacherForm";



    // People Related
    public const string HabilitationComprovative = "HabilitationComprovative";
    public const string IbanComprovative = "IbanComprovative";
    public const string IdentificationDocument = "IdentificationDocument";
}