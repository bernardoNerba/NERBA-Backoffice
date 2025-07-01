using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Actions.Dtos;
using NERBABO.ApiService.Core.Actions.Services;
using NERBABO.ApiService.Shared.Services;
using System.Security.Claims;

namespace NERBABO.ApiService.Core.Actions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionsController(
        ICourseActionService courseActionService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly ICourseActionService _courseActionService = courseActionService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateActionAsync([FromBody] CreateCourseActionDto createCourseActionDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized("Efetue autenticação.");
            }

            createCourseActionDto.CoordenatorId = userId;

            var result = await _courseActionService.CreateAsync(createCourseActionDto);
            return _responseHandler.HandleResult(result);
        }

        [HttpDelete("{id:long}")]
        [Authorize]
        public async Task<IActionResult> DeleteActionIfCoordenatorAsync(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized("Efetue autenticação.");
            }

            var result = await _courseActionService.DeleteIfCoordenatorAsync(id, userId);
            return _responseHandler.HandleResult(result);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllActionsAsync()
        {
            var result = await _courseActionService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("{id:long}")]
        [Authorize]
        public async Task<IActionResult> GetActionByIdAsync(long id)
        {
            var result = await _courseActionService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }



    }
}
