using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Modules.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleController(
        IModuleService moduleService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly IModuleService _moduleService = moduleService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Retrieves all modules.
        /// </summary>
        /// <response code="200">There are modules in the system. Returns the list of
        /// RetrieveModuleDto.</response>
        /// <response code="404">There are no modules in the system.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is
        /// not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllModulesAsync()
        {
            Result<IEnumerable<RetrieveModuleDto>> result = await _moduleService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetModuleById(long id)
        {
            Result<RetrieveModuleDto> result = await _moduleService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("active")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetActiveModulesAsync()
        {
            Result<IEnumerable<RetrieveModuleDto>> result = await _moduleService.GetActiveModulesAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> CreateModuleAsync([FromBody] CreateModuleDto moduleDto)
        {
            Result<RetrieveModuleDto> result = await _moduleService.CreateAsync(moduleDto);
            return _responseHandler.HandleResult(result);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateModuleAsync(long id, [FromBody] UpdateModuleDto moduleDto)
        {
            if (id != moduleDto.Id)
                return BadRequest("ID Missmatch");
            Result<RetrieveModuleDto> result = await _moduleService.UpdateAsync(moduleDto);
            return _responseHandler.HandleResult(result);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteModuleAsync(long id)
        {
            Result result = await _moduleService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}
