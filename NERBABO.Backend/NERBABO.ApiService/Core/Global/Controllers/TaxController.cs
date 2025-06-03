using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class TaxController : ControllerBase
    {
        private readonly ITaxService _TaxService;
        private readonly ILogger<TaxController> _logger;
        private readonly UserManager<User> _userManager;
        public TaxController(
            ITaxService TaxService,
            ILogger<TaxController> logger,
            UserManager<User> userManager)
        {
            _TaxService = TaxService;
            _logger = logger;
            _userManager = userManager;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RetrieveTaxDto>>> GetAllTaxesAsync()
        {
            try
            {
                var taxes = await _TaxService.GetAllTaxesAsync();
                if (!taxes.Any())
                {
                    _logger.LogError("There is none  taxes, did you forget to load them?");
                    return NotFound("Não foram encontradas taxas");
                }

                return Ok(taxes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Fetching all taxes.");
                return BadRequest("Erro ao obter taxas");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<ActionResult> CreateTaxAsync([FromBody] CreateTaxDto tax)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");

            // Check if the user is null or if they are not an admin
            if (user == null || !await user.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            try
            {
                await _TaxService.CreateTaxAsync(tax);
                return Ok(new OkMessage(
                    $"Taxa {tax.Name} criada com sucesso.",
                    "Taxa criada com sucesso.",
                    true
                ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating tax.");
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id:int}")]
        public async Task<ActionResult> UpdateTaxAsync(int id, [FromBody] UpdateTaxDto tax)
        {
            if (id != tax.Id)
                return BadRequest("Id mismatch.");

            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");
            // Check if the user is null or if they are not an admin
            if (user == null || !await user.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            try
            {
                await _TaxService.UpdateTaxAsync(tax);
                return Ok(new OkMessage(
                    $"Taxa {tax.Name} atualizada com sucesso.",
                    "Taxa atualizada com sucesso.",
                    true
                ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating taxa.");
                return BadRequest("Ocorreu um erro inesperado ao atualizar a taxa.");
            }

        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:int}")]
        public async Task<ActionResult> DeleteTaxAsync(int id)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");
            // Check if the user is null or if they are not an admin
            if (user == null || !await user.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }
            try
            {
                await _TaxService.DeleteTaxAsync(id);
                return Ok(new OkMessage(
                    $"Taxa {id} eliminada com sucesso.",
                    "Taxa eliminada com sucesso.",
                    true
                ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting tax");
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<RetrieveTaxDto>>> GetTaxesByTypeAsync(string type)
        {
            try
            {
                var taxes = await _TaxService.GetTaxesByTypeAsync(type);
                if (!taxes.Any())
                {
                    _logger.LogError($"There is no taxes of type {type}, did you forget to load them?");
                    return NotFound($"Não foram encontradas taxas do tipo {type}");
                }

                return Ok(taxes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Fetching taxes by type.");
                return BadRequest("Erro ao obter taxas por tipo");
            }
        }
    }
}
