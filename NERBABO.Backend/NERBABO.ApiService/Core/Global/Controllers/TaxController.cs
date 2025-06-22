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
    public class TaxController(
        ITaxService taxService,
        IResponseHandler responseHandler) : ControllerBase
    {
        private readonly ITaxService _TaxService = taxService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllTaxesAsync()
        {
            Result<IEnumerable<RetrieveTaxDto>> result = await _TaxService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> CreateTaxAsync([FromBody] CreateTaxDto tax)
        {
            Result<RetrieveTaxDto> result = await _TaxService.CreateAsync(tax);
            return _responseHandler.HandleResult(result);
        }

        [HttpPut("update/{id:int}")]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateTaxAsync(int id, [FromBody] UpdateTaxDto tax)
        {
            if (id != tax.Id)
                return BadRequest("Id mismatch.");

            Result<RetrieveTaxDto> result = await _TaxService.UpdateAsync(tax);
            return _responseHandler.HandleResult(result);

        }

        [HttpDelete("delete/{id:int}")]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteTaxAsync(int id)
        {
            Result result = await _TaxService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("type/{type}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetTaxesByTypeAndActiveAsync(string type)
        {
            Result<IEnumerable<RetrieveTaxDto>> result = await _TaxService.GetByTypeAndIsActiveAsync(type);
            return _responseHandler.HandleResult(result);
        }
    }
}
