using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
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
        {
            var userResult = Result<byte[]>
                .Fail("Utilizador inválido.", "Utilizador não autenticado.",
                    StatusCodes.Status401Unauthorized);
            return _responseHandler.HandleResult(userResult);
        }

        var result = await _pdfService.GenerateSessionReportAsync(actionId, user.Id);

        if (!result.Success)
        {
            return _responseHandler.HandleResult(result);
        }

        return File(result.Data!, "application/pdf", $"sessoes-acao-{actionId}-{DateTime.Now:yyyyMMdd}.pdf");
    }

    /// <summary>
    /// Generates or returns cached PDF cover page for a specific action.
    /// </summary>
    /// <param name="actionId">The action ID to generate the cover for.</param>
    /// <response code="200">PDF generated successfully. Returns the PDF file.</response>
    /// <response code="404">Action not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred during PDF generation.</response>
    [HttpGet("action/{actionId:long}/cover-report")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GenerateCoverReportAsync(long actionId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            var userResult = Result<byte[]>
                .Fail("Utilizador inválido.", "Utilizador não autenticado.", StatusCodes.Status401Unauthorized);
            return _responseHandler.HandleResult(userResult);
        }

        var result = await _pdfService.GenerateCoverReportAsync(actionId, user.Id);

        if (!result.Success)
        {
            return _responseHandler.HandleResult(result);
        }

        return File(result.Data!, "application/pdf", $"capa-acao-{actionId}-{DateTime.Now:yyyyMMdd}.pdf");
    }

    /// <summary>
    /// Generates or returns cached PDF teacher form (Ficha de Formador) for a specific teacher in an action.
    /// </summary>
    /// <param name="actionId">The action ID to generate the form for.</param>
    /// <param name="teacherId">The teacher ID to generate the form for.</param>
    /// <response code="200">PDF generated successfully. Returns the PDF file.</response>
    /// <response code="404">Action or teacher not found, or teacher has no modules in this action.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred during PDF generation.</response>
    [HttpGet("action/{actionId:long}/teacher-form/{teacherId:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GenerateTeacherFormAsync(long actionId, long teacherId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            var userResult = Result<byte[]>
                .Fail("Utilizador inválido.", "Utilizador não autenticado.", StatusCodes.Status401Unauthorized);
            return _responseHandler.HandleResult(userResult);
        }

        var result = await _pdfService.GenerateTeacherFormAsync(actionId, teacherId, user.Id);

        if (!result.Success)
        {
            return _responseHandler.HandleResult(result);
        }

        return File(result.Data!, "application/pdf", $"ficha-formador-{teacherId}-acao-{actionId}-{DateTime.Now:yyyyMMdd}.pdf");
    }

    /// <summary>
    /// Generates or returns cached PDF course action information report for a specific action.
    /// </summary>
    /// <param name="actionId">The action ID to generate the report for.</param>
    /// <response code="200">PDF generated successfully. Returns the PDF file.</response>
    /// <response code="404">Action not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred during PDF generation.</response>
    [HttpGet("action/{actionId:long}/course-action-information-report")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GenerateTrainingFinancingFormAsync(long actionId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            var userResult = Result<byte[]>
                .Fail("Utilizador inválido.", "Utilizador não autenticado.", StatusCodes.Status401Unauthorized);
            return _responseHandler.HandleResult(userResult);
        }

        var result = await _pdfService.GenerateTrainingFinancingFormAsync(actionId, user.Id);

        if (!result.Success)
        {
            return _responseHandler.HandleResult(result);
        }

        return File(result.Data!, "application/pdf", $"informacao-acao-{actionId}-{DateTime.Now:yyyyMMdd}.pdf");
    }

    /// <summary>
    /// Generates or returns cached PDF course action process student payments for a specific action.
    /// </summary>
    /// <param name="actionId">The action ID to generate the report for.</param>
    /// <response code="200">PDF generated successfully. Returns the PDF file.</response>
    /// <response code="404">Action not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred during PDF generation.</response>
    [HttpGet("action/{actionId:long}/course-action-student-payments-report")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GenerateCourseActionProcessStudentPaymentsReportAsync(long actionId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            var userResult = Result<byte[]>
                .Fail("Utilizador inválido.", "Utilizador não autenticado.", StatusCodes.Status401Unauthorized);
            return _responseHandler.HandleResult(userResult);
        }

        var result = await _pdfService.GenerateCourseActionProcessStudentPaymentsAsync(actionId, user.Id);

        if (!result.Success)
        {
            return _responseHandler.HandleResult(result);
        }

        return File(result.Data!, "application/pdf", $"students-process-payments-{actionId}-{DateTime.Now:yyyyMMdd}.pdf");
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
        var result = await _pdfService.GetSavedPdfAsync(pdfType, referenceId);

        if (!result.Success)
        {
            // Return a success result with exists = false when PDF is not found
            if (result.StatusCode == StatusCodes.Status404NotFound)
            {
                var notFoundResponse = new { exists = false, savedPdf = (object?)null };
                var successResult = Result<object>
                    .Ok(notFoundResponse, "Consulta realizada.", "PDF guardado não encontrado.");
                return _responseHandler.HandleResult(successResult);
            }
            return _responseHandler.HandleResult(result);
        }

        var responseData = new
        {
            exists = true,
            savedPdf = new
            {
                result.Data!.Id,
                result.Data.PdfType,
                result.Data.ReferenceId,
                result.Data.FileName,
                result.Data.FileSizeBytes,
                result.Data.GeneratedAt,
                result.Data.GeneratedByUserId
            }
        };

        var checkResult = Result<object>
            .Ok(responseData, "PDF encontrado.", "PDF guardado encontrado com sucesso.");
        return _responseHandler.HandleResult(checkResult);
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
        var result = await _pdfService.GetSavedPdfContentAsync(savedPdfId);

        if (!result.Success)
        {
            return _responseHandler.HandleResult(result);
        }

        return File(result.Data!, "application/pdf", $"saved-pdf-{savedPdfId}.pdf");
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
        var result = await _pdfService.DeleteSavedPdfAsync(savedPdfId);
        return _responseHandler.HandleResult(result);
    }
}