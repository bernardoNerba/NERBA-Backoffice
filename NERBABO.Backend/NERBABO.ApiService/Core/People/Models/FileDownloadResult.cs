namespace NERBABO.ApiService.Core.People.Models;

public class FileDownloadResult
{
    public byte[] Content { get; set; } = [];
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}