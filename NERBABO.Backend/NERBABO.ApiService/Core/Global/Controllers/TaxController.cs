using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        /// <summary>
        /// Retrieves all taxes.
        /// </summary>
        /// <response code="200">There are taxes in the system. Returns the list of RetrieveTaxDto.</response>
        /// <response code="404">There are no taxes in the system.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllTaxesAsync()
        {
            Result<IEnumerable<RetrieveTaxDto>> result = await _TaxService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Creates a new tax.
        /// </summary>
        /// <param name="tax">The CreateTaxDto object that will be validated and used to create the tax.</param>
        /// <response code="201">Created Successfully. Returns the RetrieveTaxDto created object.</response>
        /// <response code="400">Validation ERROR when validating tax type, name.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or is not active.</response>        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost("create")]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> CreateTaxAsync([FromBody] CreateTaxDto tax)
        {
            Result<RetrieveTaxDto> result = await _TaxService.CreateAsync(tax);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Update a tax by its ID.
        /// </summary>
        /// <param name="id">The ID of the tax to be updated.</param>
        /// <param name="tax">The UpdateTaxDto object that will be validated and used to update the tax.</param>
        /// <response code="200">Updated Successfully. Returns the RetrieveTaxDto updated object.</response>
        /// <response code="400">Validation ERROR when validating tax type, name or ID mismatch.</response>
        /// <response code="404">The tax with the given ID does not exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("{id:int}")]
        [HttpPut("update/{id:int}")]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateTaxAsync(int id, [FromBody] UpdateTaxDto tax)
        {
            if (id != tax.Id)
                return BadRequest("Id mismatch.");

            Result<RetrieveTaxDto> result = await _TaxService.UpdateAsync(tax);
            return _responseHandler.HandleResult(result);

        }

        /// <summary>
        /// Deletes a tax by its ID.
        /// </summary>
        /// <param name="id">The ID of the tax to be deleted.</param>
        /// <response code="200">Deleted Successfully.</response>
        /// <response code="404">The tax with the given ID does not exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("delete/{id:int}")]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteTaxAsync(int id)
        {
            Result result = await _TaxService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Retrieves taxes by type and active status.
        /// </summary>
        /// <param name="type">The type of tax to filter by.</param>
        /// <response code="200">There are taxes of the specified type and active status.
        /// Returns the list of RetrieveTaxDto.</response>
        /// <response code="404">There are no taxes of the specified type and active status.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("type/{type}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetTaxesByTypeAndActiveAsync(string type)
        {
            Result<IEnumerable<RetrieveTaxDto>> result = await _TaxService.GetByTypeAndIsActiveAsync(type);
            return _responseHandler.HandleResult(result);
        }
    }
}
