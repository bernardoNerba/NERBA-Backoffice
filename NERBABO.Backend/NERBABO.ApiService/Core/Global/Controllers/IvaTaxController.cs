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
    public class IvaTaxController : ControllerBase
    {
        private readonly IIvaTaxService _ivaTaxService;
        private readonly ILogger<IvaTaxController> _logger;
        private readonly UserManager<User> _userManager;
        public IvaTaxController(
            IIvaTaxService ivaTaxService,
            ILogger<IvaTaxController> logger,
            UserManager<User> userManager)
        {
            _ivaTaxService = ivaTaxService;
            _logger = logger;
            _userManager = userManager;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RetrieveIvaTaxDto>>> GetAllIvaTaxesAsync()
        {
            try
            {
                var taxes = await _ivaTaxService.GetAllIvaTaxesAsync();
                if (!taxes.Any())
                {
                    _logger.LogError("There is none iva taxes, did you forget to load them?");
                    return NotFound("Não foram encontradas taxas de iva");
                }

                return Ok(taxes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Fetching all iva taxes.");
                return BadRequest("Erro ao obter taxas de iva");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<ActionResult> CreateIvaTaxAsync([FromBody] CreateIvaTaxDto taxIva)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");

            // Check if the user is null or if they are not an admin
            if (!await user!.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }

            try
            {
                await _ivaTaxService.CreateTaxIvaAsync(taxIva);
                return Ok(new OkMessage(
                    $"Taxa Iva {taxIva.Name} criada com sucesso.",
                    "Taxa Iva criada com sucesso.",
                    true
                ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating taxa iva");
                return BadRequest(e.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:int}")]
        public async Task<ActionResult> DeleteTaxIvaAsync(int id)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value ?? "");
            // Check if the user is null or if they are not an admin
            if (!await user!.CheckUserHasRoleAndActive("Admin", _userManager, _logger))
            {
                return Unauthorized("Não está autorizado a aceder a esta informação.");
            }
            try
            {
                await _ivaTaxService.DeleteTaxIvaAsync(id);
                return Ok(new OkMessage(
                    $"Taxa Iva {id} eliminada com sucesso.",
                    "Taxa Iva eliminada com sucesso.",
                    true
                ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deleting taxa iva");
                return BadRequest(e.Message);
            }
        }
    }
}
