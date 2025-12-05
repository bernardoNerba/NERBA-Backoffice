using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Shared.BulkImport.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Shared.BulkImport.Services;

public interface IBulkImportService<TDto, TEntity, TRetrieveDto>
{
    /// <summary>
    /// Imports multiple entities from a file
    /// </summary>
    Task<Result<BulkImportResult<TRetrieveDto>>> ImportFromFileAsync(
        IFormFile file,
        FileType fileType,
        ImportOptions options);

    /// <summary>
    /// Downloads a template file for import
    /// </summary>
    Task<Result<FileDownloadResult>> GetTemplateFileAsync(FileType fileType);

    /// <summary>
    /// Validates file without importing (dry-run)
    /// </summary>
    Task<Result<BulkImportResult<TRetrieveDto>>> ValidateImportAsync(
        IFormFile file,
        FileType fileType);
}

public class ImportOptions
{
    /// <summary>
    /// Stop import on first error (default: false - continue with valid rows)
    /// </summary>
    public bool StopOnFirstError { get; set; } = false;

    /// <summary>
    /// Skip rows with warnings (default: false - import with warnings)
    /// </summary>
    public bool SkipWarnings { get; set; } = false;

    /// <summary>
    /// Batch size for database commits (default: 100)
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Update existing records if unique key matches (default: false - fail on duplicates)
    /// </summary>
    public bool UpdateExisting { get; set; } = false;
}
