using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Companies.Dtos;
using NERBABO.ApiService.Core.Companies.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Companies.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController(
        ICompanyService companyService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly ICompanyService _companyService = companyService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Create a new company
        /// </summary>
        /// <param name="companyDto">The CreateCompanyDto object that will be validated and
        /// used to create the company.</param>
        /// <response code="201">Created Successfully. Returns the RetrieveCompanyDto created object.</response>
        /// <response code="400">Validation ERROR when validating company name duplication.</response>
        /// <response code="400">Validation ERROR when validating company email duplication.</response>
        /// <response code="400">Validation ERROR when validating company zip code duplication.</response>
        /// <response code="400">Validation ERROR when validating company phone number duplication.</response>
        /// <response code="400">Validation ERROR when validating ativity sector.</response>
        /// <response code="400">Validation ERROR when validating company size.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [ProducesResponseType(typeof(Result<RetrieveCompanyDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Result<RetrieveCompanyDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "ActiveUser")]
        [HttpPost]
        public async Task<IActionResult> CreateCompanyAsync([FromBody] CreateCompanyDto companyDto)
        {
            Result<RetrieveCompanyDto> result = await _companyService.CreateAsync(companyDto);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Get a company by its ID
        /// </summary>
        /// <param name="id">The ID of the company to be retrieved.</param>
        /// <response code="200">The company with the given ID exists. Returns the RetrieveCompanyDto object.</response>
        /// <response code="404">The company with the given ID does not exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [ProducesResponseType(typeof(Result<RetrieveCompanyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<RetrieveCompanyDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "ActiveUser")]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetCompanyByIdAsync(long id)
        {
            Result<RetrieveCompanyDto> result = await _companyService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Update a company by its ID
        /// </summary>
        /// <param name="id">The ID of the company to be updated.</param>
        /// <param name="companyDto">The UpdateCompanyDto object that will be validated and
        /// used to update the company.</param>
        /// <response code="200">Updated Successfully. Returns the RetrieveCompanyDto updated object.</response>
        /// <response code="400">Validation ERROR when validating company name duplication.</response>
        /// <response code="400">Validation ERROR when validating company email duplication.</response>
        /// <response code="400">Validation ERROR when validating company zip code duplication.</response>
        /// <response code="400">Validation ERROR when validating company phone number duplication.</response>
        /// <response code="400">Validation ERROR when validating ativity sector.</response>
        /// <response code="400">Validation ERROR when validating company size.</response>
        /// <response code="404">The company with the given ID does not exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is
        /// not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [ProducesResponseType(typeof(Result<RetrieveCompanyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<RetrieveCompanyDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<RetrieveCompanyDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "ActiveUser")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateCompanyAsync(long id, [FromBody] UpdateCompanyDto companyDto)
        {
            if (id != companyDto.Id) return BadRequest("ID Missmatch");
            Result<RetrieveCompanyDto> result = await _companyService.UpdateAsync(companyDto);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Get all companies in the system.
        /// </summary>
        /// <response code="200">There are companies in the system. Returns the List of RetrieveCompanyDto objects.</response>
        /// <response code="404">There are no companies in the system.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is
        /// not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [ProducesResponseType(typeof(Result<RetrieveCompanyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<RetrieveCompanyDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Result<RetrieveCompanyDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "ActiveUser")]
        [HttpGet]
        public async Task<IActionResult> GetAllCompaniesAsync()
        {
            Result<IEnumerable<RetrieveCompanyDto>> result = await _companyService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Delete a company by its ID
        /// </summary>
        /// <param name="id">The ID of the company to be deleted.</param>
        /// <response code="200">Deleted Successfully.</response>
        /// <response code="404">The company with the given ID does not exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [Authorize(Policy = "ActiveUser")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DelteCompanyAsync(long id)
        {
            Result result = await _companyService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}
