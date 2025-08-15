
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Sessions.Dtos;
using NERBABO.ApiService.Core.Sessions.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Sessions.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SessionsController(
    ISessionService sessionService,
    IResponseHandler responseHandler,
    UserManager<User> userManager
    ) : ControllerBase
{
    private readonly ISessionService _sessionService = sessionService;

    private readonly IResponseHandler _responseHandler = responseHandler;
    private readonly UserManager<User> _userManager = userManager;

    /// <summary>
    /// Creates a new session.
    /// </summary>
    /// <param name="createSessionDto">The data for the new session.</param>
    /// <response code="201">Session created successfully. Returns a RetrieveSessionDto.</response>
    /// <response code="400">Invalid session data provided.</response>
    /// <response code="404">Related ModuleTeaching not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpPost]
    [Authorize(Policy = "ActiveUser", Roles = "Admin, FM")]
    public async Task<IActionResult> CreateSessionAsync([FromBody] CreateSessionDto createSessionDto)
    {
        // pick the user id from the claims and assing it to the create session dto
        // the service will need to check if user is the action coordenator
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return BadRequest("Invalid User");
        createSessionDto.User = user;
        Result<RetrieveSessionDto> result = await _sessionService.CreateAsync(createSessionDto);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Updates an existing session.
    /// </summary>
    /// <param name="updateSessionDto">The data to update the session with.</param>
    /// <response code="200">Session updated successfully. Returns a RetrieveSessionDto.</response>
    /// <response code="400">Invalid session data provided.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="401">Unauthorized access. User is not the action coordinator or admin.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpPut]
    [Authorize(Policy = "ActiveUser", Roles = "Admin, FM")]
    public async Task<IActionResult> UpdateSessionAsync([FromBody] UpdateSessionDto updateSessionDto)
    {
        // pick the user id from the claims and assign it to the update session dto
        // the service will need to check if user is the action coordinator
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return BadRequest("Invalid User");
        updateSessionDto.User = user;
        
        Result<RetrieveSessionDto> result = await _sessionService.UpdateAsync(updateSessionDto);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Gets all sessions in the system.
    /// </summary>
    /// <response code="200">Sessions retrieved successfully. Returns a collection of RetrieveSessionDto.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetAllSessionsAsync()
    {
        Result<IEnumerable<RetrieveSessionDto>> result = await _sessionService.GetAllAsync();
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Gets all sessions for a specific action.
    /// </summary>
    /// <param name="id">The action ID to get sessions for.</param>
    /// <response code="200">Sessions retrieved successfully. Returns a collection of RetrieveSessionDto.</response>
    /// <response code="404">Action not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("action/{id:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetAllSessionsByActionIdAsync(long id)
    {
        Result<IEnumerable<RetrieveSessionDto>> result = await _sessionService.GetAllByActionIdAsync(id);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Deletes a session. Only the action coordinator or admin can delete sessions.
    /// </summary>
    /// <param name="id">The session ID to delete.</param>
    /// <response code="200">Session deleted successfully.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="400">Session cannot be deleted (e.g., already taught).</response>
    /// <response code="401">Unauthorized access. User is not the action coordinator or admin.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = "ActiveUser", Roles = "Admin, FM")]
    public async Task<IActionResult> DeleteSessionIfActionCoordenatorAsync(long id)
    {
        // pick the user id from the claims and assign it to the delete operation
        // the service will need to check if user is the action coordinator
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return BadRequest("Invalid User");

        Result result = await _sessionService.DeleteIfActionCoordenatorAsync(id, user);
        return _responseHandler.HandleResult(result);
    }
    
    /// <summary>
    /// Gets a specific session by its ID.
    /// </summary>
    /// <param name="id">The session ID to retrieve.</param>
    /// <response code="200">Session retrieved successfully. Returns a RetrieveSessionDto.</response>
    /// <response code="404">Session not found.</response>
    /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("{id:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetSessionByIdAsync(long id)
    {   
        Result<RetrieveSessionDto> result = await _sessionService.GetByIdAsync(id);
        return _responseHandler.HandleResult(result);
    }
}
