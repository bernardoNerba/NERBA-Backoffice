using System;
using System.Security.Cryptography;
using System.Text;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Reports.Models;

public class Report : Entity<Guid>
{
    /// <summary>
    /// The Report type, one of the ReportTypes class properties
    /// </summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>
    /// EntityId like ActionId or SessionId 
    /// </summary>
    public long ReferenceId { get; set; } // ActionId or SessionId

    /// <summary>
    /// The file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The file stored path
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of bytes that the file occupates
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// SHA256 hash of content for change detection
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// The User Id who generated the report
    /// </summary>
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
        StringBuilder builder = new ();

        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString();
    }
}