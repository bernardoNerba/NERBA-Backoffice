using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Core.Global.Services;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Global.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralInfoController : ControllerBase
    {

        private readonly IGeneralInfoService _generalInfoService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<GeneralInfoController> _logger;
        public GeneralInfoController(
            IGeneralInfoService generalInfoService,
            UserManager<User> userManager,
            ILogger<GeneralInfoController> logger)
        {
            _generalInfoService = generalInfoService;
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet()]
        public async Task<ActionResult<RetrieveGeneralInfoDto>> GetGeneralInfoAsync()
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await user!.CheckUserHasRoleAndActive("Admin", _userManager);

            var config = await _generalInfoService.GetGeneralInfoAsync();
            if (config == null)
            {
                _logger.LogWarning("Is possible that there is no general information configuration.");
                return NotFound("Não foi encontrada nenhum configuração geral no sistema.");
            }
            return Ok(config);

        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        public async Task<ActionResult<RetrieveGeneralInfoDto>> UpdateGeneralInfoAsync([FromBody] UpdateGeneralInfoDto updateConfig)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await user!.CheckUserHasRoleAndActive("Admin", _userManager);

            await _generalInfoService.UpdateGeneralInfoAsync(updateConfig);
            return Ok(new OkMessage(
                $"Configurações Gerais Atualizadas.",
                "Foram atualizadas as configurações gerais.",
                true
            ));

        }
    }
}
