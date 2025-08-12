
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

    [HttpGet("action/{id:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetAllSessionsByActionIdAsync(long id)
    {
        Result<IEnumerable<RetrieveSessionDto>> result = await _sessionService.GetAllByActionIdAsync(id);
        return _responseHandler.HandleResult(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = "ActiveUser", Roles = "Admin, FM")]
    public async Task<IActionResult> DeleteSessionIfActionCoordenatorAsync(long id)
    {
        // pick the user id from the claims and assing it to the create session dto
        // the service will need to check if user is the action coordenator
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return BadRequest("Invalid User");
        
        Result result = await _sessionService.DeleteIfActionCoordenatorAsync(id, user);
        return _responseHandler.HandleResult(result);
    }
}
