using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.ModuleAvaliations.Dtos;
using NERBABO.ApiService.Core.ModuleAvaliations.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.ModuleAvaliations.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleAvaliationsController(
        IModuleAvaliationsService moduleAvaliationsService,
        IResponseHandler responseHandler
    ) : ControllerBase
    {
        private readonly IModuleAvaliationsService _moduleAvaliationsService = moduleAvaliationsService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Gets all avaliations records for a specific action.
        /// </summary>
        /// <param name="actionId">The action ID to get the avaliations for.</param>
        /// <returns></returns>
        [HttpGet("by-action/{actionId}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetModuleAvaliationsByActionIdAsync(long actionId)
        {
            Result<IEnumerable<AvaliationsByModuleDto>> result = await _moduleAvaliationsService.GetByActionIdAsync(actionId);
            return _responseHandler.HandleResult(result);
        }
    }
}
