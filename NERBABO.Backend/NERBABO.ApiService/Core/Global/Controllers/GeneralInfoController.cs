using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Global.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Global.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralInfoController : ControllerBase
    {

        private readonly IGeneralInfoService _generalInfoService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<GeneralInfoController> _logger;
        private readonly IResponseHandler _responseHandler;

        public GeneralInfoController(
            IGeneralInfoService generalInfoService,
            UserManager<User> userManager,
            ILogger<GeneralInfoController> logger,
            IResponseHandler responseHandler)
        {
            _generalInfoService = generalInfoService;
            _userManager = userManager;
            _logger = logger;
            _responseHandler = responseHandler;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetGeneralInfoAsync()
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            Result<RetrieveGeneralInfoDto> result = await _generalInfoService.GetGeneralInfoAsync();
            return _responseHandler.HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateGeneralInfoAsync([FromBody] UpdateGeneralInfoDto updateConfig)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            Result result = await _generalInfoService.UpdateGeneralInfoAsync(updateConfig);
            return _responseHandler.HandleResult(result);
        }
    }
}
