using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Reports.Models;
using NERBABO.ApiService.Core.Reports.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Reports.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PdfController(
    IPdfService pdfService,
    IResponseHandler responseHandler,
    UserManager<User> userManager
    ) : ControllerBase
{
    private readonly IPdfService _pdfService = pdfService;
    private readonly IResponseHandler _responseHandler = responseHandler;
    private readonly UserManager<User> _userManager = userManager;

    /// <summary>
    /// Generates or returns cached PDF report with all sessions for a specific action.
    /// </summary>
    /// <param name="actionId">The action ID to generate the report for.</param>
    /// <response code="200">PDF generated successfully. Returns the PDF file.</response>
    /// <response code="404">Action not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred during PDF generation.</response>
    [HttpGet("action/{actionId:long}/sessions-report")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GenerateSessionReportAsync(long actionId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return BadRequest("Invalid User");

        byte[] pdfBytes;

        pdfBytes = await _pdfService.GenerateSessionReportAsync(actionId, user.Id);
        return File(pdfBytes, "application/pdf", $"sessoes-acao-{actionId}-{DateTime.Now:yyyyMMdd}.pdf");
    }
    
    /// <summary>
    /// Checks if a saved PDF exists for the specified type and reference ID.
    /// </summary>
    /// <param name="pdfType">The PDF type (SessionReport, SessionDetail, ActionSummary).</param>
    /// <param name="referenceId">The reference ID (ActionId or SessionId).</param>
    /// <response code="200">Returns information about the saved PDF or null if not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    [HttpGet("check/{pdfType}/{referenceId:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> CheckSavedPdfAsync(string pdfType, long referenceId)
    {
        var savedPdf = await _pdfService.GetSavedPdfAsync(pdfType, referenceId);
        
        if (savedPdf is null)
        {
            return Ok(new { exists = false, savedPdf = (object?)null });
        }

        return Ok(new 
        { 
            exists = true, 
            savedPdf = new 
            {
                savedPdf.Id,
                savedPdf.PdfType,
                savedPdf.ReferenceId,
                savedPdf.FileName,
                savedPdf.FileSizeBytes,
                savedPdf.GeneratedAt,
                savedPdf.GeneratedByUserId
            }
        });
    }

    /// <summary>
    /// Downloads a saved PDF by its ID.
    /// </summary>
    /// <param name="savedPdfId">The saved PDF ID.</param>
    /// <response code="200">Returns the PDF file.</response>
    /// <response code="404">PDF not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    [HttpGet("download/{savedPdfId:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> DownloadSavedPdfAsync(long savedPdfId)
    {
        var pdfContent = await _pdfService.GetSavedPdfContentAsync(savedPdfId);
        if (pdfContent is null)
        {
            return NotFound("PDF not found");
        }

        return File(pdfContent, "application/pdf", $"saved-pdf-{savedPdfId}.pdf");
    }

    /// <summary>
    /// Deletes a saved PDF.
    /// </summary>
    /// <param name="savedPdfId">The saved PDF ID.</param>
    /// <response code="200">PDF deleted successfully.</response>
    /// <response code="404">PDF not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    [HttpDelete("{savedPdfId:long}")]
    [Authorize(Policy = "ActiveUser", Roles = "Admin, FM")]
    public async Task<IActionResult> DeleteSavedPdfAsync(long savedPdfId)
    {
        var success = await _pdfService.DeleteSavedPdfAsync(savedPdfId);
        if (!success)
        {
            return NotFound("PDF not found");
        }

        return Ok(new { message = "PDF deleted successfully" });
    }
}