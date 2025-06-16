using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class TaxController : ControllerBase
    {
        private readonly ITaxService _TaxService;
        private readonly ILogger<TaxController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IResponseHandler _responseHandler;

        public TaxController(
            ITaxService TaxService,
            ILogger<TaxController> logger,
            UserManager<User> userManager,
            IResponseHandler responseHandler)
        {
            _TaxService = TaxService;
            _logger = logger;
            _userManager = userManager;
            _responseHandler = responseHandler;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllTaxesAsync()
        {
            Result<IEnumerable<RetrieveTaxDto>> result = await _TaxService.GetAllTaxesAsync();
            return _responseHandler.HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateTaxAsync([FromBody] CreateTaxDto tax)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            Result result = await _TaxService.CreateTaxAsync(tax);
            return _responseHandler.HandleResult(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> UpdateTaxAsync(int id, [FromBody] UpdateTaxDto tax)
        {
            if (id != tax.Id)
                return BadRequest("Id mismatch.");

            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            Result result = await _TaxService.UpdateTaxAsync(tax);
            return _responseHandler.HandleResult(result);

        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> DeleteTaxAsync(int id)
        {
            // Get the user from the token
            var user = await _userManager.FindByIdAsync(User.FindFirst
                (ClaimTypes.NameIdentifier)?.Value
                ?? throw new KeyNotFoundException("Efetua autenticação antes de proceder."));

            // Check if the user is null or if they are not an admin
            await Helper.AuthHelp.CheckUserHasRoleAndActive(user!, "Admin", _userManager);

            Result result = await _TaxService.DeleteTaxAsync(id);
            return _responseHandler.HandleResult(result);

        }

        [Authorize]
        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetTaxesByTypeAndActiveAsync(string type)
        {
            Result<IEnumerable<RetrieveTaxDto>> result = await _TaxService.GetTaxesByTypeAndIsActiveAsync(type);
            return _responseHandler.HandleResult(result);
        }
    }
}
