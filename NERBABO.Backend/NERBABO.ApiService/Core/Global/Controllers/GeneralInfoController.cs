using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Global.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Global.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralInfoController(
        IGeneralInfoService generalInfoService,
        IResponseHandler responseHandler) : ControllerBase
    {

        private readonly IGeneralInfoService _generalInfoService = generalInfoService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Retrieves general information.
        /// </summary>
        /// <response code="200">General information retrieved successfully. Returns RetrieveGeneralInfoDto.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetGeneralInfoAsync()
        {
            Result<RetrieveGeneralInfoDto> result = await _generalInfoService.GetGeneralInfoAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Updates general information.
        /// </summary>
        /// <param name="updateConfig">The UpdateGeneralInfoDto object that will be validated and used to update the general information.</param>
        /// <response code="200">Updated Successfully. Returns the updated RetrieveGeneralInfoDto.</response>
        /// <response code="400">Validation ERROR when validating UpdateGeneralInfoDto.</response>
        /// <response code="404">Iva tax with given ivaId not found.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGeneralInfoAsync([FromBody] UpdateGeneralInfoDto updateConfig)
        {
            Result result = await _generalInfoService.UpdateGeneralInfoAsync(updateConfig);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Health check endpoint for application readiness and liveness.
        /// </summary>
        /// <response code="200">Application is healthy and ready to serve requests.</response>
        /// <response code="503">Application is not healthy - database or cache issues.</response>
        [HttpGet("health")]
        [AllowAnonymous]
        public async Task<IActionResult> HealthCheck()
        {
            Result<object> result = await _generalInfoService.HealthCheckAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Simple liveness probe for basic application availability.
        /// </summary>
        /// <response code="200">Application is alive and responding.</response>
        [HttpGet("alive")]
        [AllowAnonymous]
        public async Task<IActionResult> Alive()
        {
            Result<object> result = await _generalInfoService.AliveAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Readiness probe that checks if application is ready to serve traffic.
        /// </summary>
        /// <response code="200">Application is ready to serve requests.</response>
        /// <response code="503">Application is not ready - still initializing.</response>
        [HttpGet("ready")]
        [AllowAnonymous]
        public async Task<IActionResult> Ready()
        {
            Result<object> result = await _generalInfoService.ReadyAsync();
            return _responseHandler.HandleResult(result);
        }
    }
}
