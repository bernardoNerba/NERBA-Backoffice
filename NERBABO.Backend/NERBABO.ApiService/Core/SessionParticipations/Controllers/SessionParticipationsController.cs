using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.SessionParticipations.Dtos;
using NERBABO.ApiService.Core.SessionParticipations.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.SessionParticipations.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SessionParticipationsController(
    ISessionParticipationService sessionParticipationService,
    IResponseHandler responseHandler
) : ControllerBase
{
    private readonly ISessionParticipationService _sessionParticipationService = sessionParticipationService;
    private readonly IResponseHandler _responseHandler = responseHandler;


    /// <summary>
    /// Gets all session participation records for a specific session.
    /// </summary>
    /// <param name="sessionId">The session ID to get participations for.</param>
    /// <response code="200">Session participations retrieved successfully. Returns a collection of RetrieveSessionParticipationDto.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("session/{sessionId:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetSessionParticipationsBySessionIdAsync(long sessionId)
    {
        Result<IEnumerable<RetrieveSessionParticipationDto>> result = await _sessionParticipationService.GetBySessionIdAsync(sessionId);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Gets all session participation records for a specific action.
    /// </summary>
    /// <param name="actionId">The action ID to get participations for.</param>
    /// <response code="200">Session participations retrieved successfully. Returns a collection of RetrieveSessionParticipationDto.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("action/{actionId:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetSessionParticipationsByActionIdAsync(long actionId)
    {
        Result<IEnumerable<RetrieveSessionParticipationDto>> result = await _sessionParticipationService.GetByActionIdAsync(actionId);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Upserts (creates or updates) session attendance for multiple students in a session.
    /// This is the main endpoint for batch attendance management.
    /// </summary>
    /// <param name="upsertDto">The data containing session ID and student attendance records.</param>
    /// <response code="200">Session attendance upserted successfully. Returns a collection of RetrieveSessionParticipationDto.</response>
    /// <response code="400">Invalid attendance data provided.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpPost("upsert-attendance")]
    [Authorize(Policy = "ActiveUser", Roles = "Admin, FM")]
    public async Task<IActionResult> UpsertSessionAttendanceAsync([FromBody] UpsertSessionAttendanceDto upsertDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized("Efetue autenticação.");
            }

            upsertDto.UserId = userId;
    
        Result<IEnumerable<RetrieveSessionParticipationDto>> result = await _sessionParticipationService.UpsertSessionAttendanceAsync(upsertDto);
        return _responseHandler.HandleResult(result);
    }
}