using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.BulkImport.Services;

public interface IFileParserService
{
    /// <summary>
    /// Parses a file and returns rows as dictionaries (header -> value)
    /// </summary>
    Task<Result<List<Dictionary<string, string>>>> ParseFileAsync(
        IFormFile file,
        FileType fileType);

    /// <summary>
    /// Validates file format and structure
    /// </summary>
    Result ValidateFileStructure(IFormFile file, FileType fileType, List<string> requiredHeaders);
}

public enum FileType
{
    CSV,
    Excel
}
