
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Sessions.Dtos;
using NERBABO.ApiService.Core.Sessions.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Sessions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController(
        ISessionService sessionService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly ISessionService _sessionService = sessionService;
        private readonly IResponseHandler _responseHandler = responseHandler;
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
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> CreateSessionAsync([FromBody] CreateSessionDto createSessionDto)
        {
            Result<RetrieveSessionDto> result = await _sessionService.CreateAsync(createSessionDto);
            return _responseHandler.HandleResult(result);
        }
    }
}