using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Services;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Modules.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleController(
        IModuleService moduleService,
        IResponseHandler responseHandler,
        ) : ControllerBase
    {
        private readonly IModuleService _moduleService = moduleService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        [HttpGet]
        [Authorize(Roles = "Admin, FM")]
        public async Task<IActionResult> GetAllModulesAsync()
        {
            var result = await _moduleService.GetAllModulesAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = "Admin, FM")]
        public async Task<IActionResult> GetModuleById(long id)
        {
            var result = await _moduleService.GetModuleByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Admin, FM")]
        public async Task<IActionResult> GetActiveModulesAsync()
        {
            var result = await _moduleService.GetActiveModulesAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, FM")]
        public async Task<IActionResult> CreateModuleAsync([FromBody] CreateModuleDto moduleDto)
        {
            var result = await _moduleService.CreateModuleAsync(moduleDto);
            return _responseHandler.HandleResult(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin, FM")]
        public async Task<IActionResult> UpdateModuleAsync([FromBody] UpdateModuleDto moduleDto)
        {
            var result = await _moduleService.UpdateModuleAsync(moduleDto);
            return _responseHandler.HandleResult(result);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin, FM")]
        public async Task<IActionResult> DeleteModuleAsync([FromQuery] long id)
        {
            var result = await _moduleService.DeleteModuleAsync(id);
            return _responseHandler.HandleResult(result);


        }
}
