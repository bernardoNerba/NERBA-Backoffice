using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
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
        IResponseHandler responseHandler,
        UserManager<User> userManager
        ) : ControllerBase
    {
        private readonly ICourseActionService _courseActionService = courseActionService;
        private readonly IResponseHandler _responseHandler = responseHandler;
        private readonly UserManager<User> _userManager = userManager;

        [HttpPost]
        [Authorize()]
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

    }
}
