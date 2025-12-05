using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.People.BulkImport.Services;
using NERBABO.ApiService.Shared.BulkImport.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.People.Controllers;

[Route("api/people/import")]
[ApiController]
public class PeopleImportController : ControllerBase
{
    private readonly IPeopleBulkImportService _importService;
    private readonly IResponseHandler _responseHandler;
    private readonly ILogger<PeopleImportController> _logger;

    public PeopleImportController(
        IPeopleBulkImportService importService,
        IResponseHandler responseHandler,
        ILogger<PeopleImportController> logger)
    {
        _importService = importService;
        _responseHandler = responseHandler;
        _logger = logger;
    }

    /// <summary>
    /// Imports multiple people from a CSV file
    /// </summary>
    /// <param name="file">CSV file with people data</param>
    /// <param name="stopOnFirstError">Stop import on first error (default: false)</param>
    /// <param name="batchSize">Number of records to commit per batch (default: 100)</param>
    /// <response code="200">Import completed (check result for individual row status)</response>
    /// <response code="400">Invalid file structure or format</response>
    /// <response code="401">Unauthorized access</response>
    [HttpPost("csv")]
    [Authorize(Policy = "ActiveUser")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    public async Task<IActionResult> ImportFromCsvAsync(
        IFormFile file,
        [FromQuery] bool stopOnFirstError = false,
        [FromQuery] int batchSize = 100)
    {
        var options = new ImportOptions
        {
            StopOnFirstError = stopOnFirstError,
            BatchSize = batchSize
        };

        var result = await _importService.ImportFromFileAsync(file, FileType.CSV, options);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Imports multiple people from an Excel file
    /// </summary>
    /// <param name="file">Excel file (.xlsx or .xls) with people data</param>
    /// <param name="stopOnFirstError">Stop import on first error (default: false)</param>
    /// <param name="batchSize">Number of records to commit per batch (default: 100)</param>
    /// <response code="200">Import completed (check result for individual row status)</response>
    /// <response code="400">Invalid file structure or format</response>
    /// <response code="401">Unauthorized access</response>
    [HttpPost("excel")]
    [Authorize(Policy = "ActiveUser")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_000_000)] // 10MB limit
    public async Task<IActionResult> ImportFromExcelAsync(
        IFormFile file,
        [FromQuery] bool stopOnFirstError = false,
        [FromQuery] int batchSize = 100)
    {
        var options = new ImportOptions
        {
            StopOnFirstError = stopOnFirstError,
            BatchSize = batchSize
        };

        var result = await _importService.ImportFromFileAsync(file, FileType.Excel, options);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Validates import file without saving (dry-run)
    /// </summary>
    [HttpPost("validate/csv")]
    [Authorize(Policy = "ActiveUser")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ValidateCsvAsync(IFormFile file)
    {
        var result = await _importService.ValidateImportAsync(file, FileType.CSV);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Validates Excel import file without saving (dry-run)
    /// </summary>
    [HttpPost("validate/excel")]
    [Authorize(Policy = "ActiveUser")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ValidateExcelAsync(IFormFile file)
    {
        var result = await _importService.ValidateImportAsync(file, FileType.Excel);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Downloads CSV template for people import
    /// </summary>
    [HttpGet("template/csv")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> DownloadCsvTemplateAsync()
    {
        var result = await _importService.GetTemplateFileAsync(FileType.CSV);
        if (!result.Success)
            return _responseHandler.HandleResult(result);

        var fileResult = result.Data!;
        return File(fileResult.Content, fileResult.ContentType, fileResult.FileName);
    }

    /// <summary>
    /// Downloads Excel template for people import
    /// </summary>
    [HttpGet("template/excel")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> DownloadExcelTemplateAsync()
    {
        var result = await _importService.GetTemplateFileAsync(FileType.Excel);
        if (!result.Success)
            return _responseHandler.HandleResult(result);

        var fileResult = result.Data!;
        return File(fileResult.Content, fileResult.ContentType, fileResult.FileName);
    }
}
