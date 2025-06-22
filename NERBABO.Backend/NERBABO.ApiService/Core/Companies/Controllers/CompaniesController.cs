using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.Internal;
using Microsoft.AspNetCore.Http;
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

        [Authorize(Policy = "ActiveUser")]
        [HttpPost]
        public async Task<IActionResult> CreateCompanyAsync([FromBody] CreateCompanyDto companyDto)
        {
            Result<RetrieveCompanyDto> result = await _companyService.CreateAsync(companyDto);
            return _responseHandler.HandleResult(result);
        }

        [Authorize(Policy = "ActiveUser")]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetCompanyByIdAsync(long id)
        {
            Result<RetrieveCompanyDto> result = await _companyService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        [Authorize(Policy = "ActiveUser")]
        [HttpPut("{id:long}")]
        public async Task<IActionResult> UpdateCompanyAsync(long id, [FromBody] UpdateCompanyDto companyDto)
        {
            if (id != companyDto.Id) return BadRequest("ID Missmatch");
            Result<RetrieveCompanyDto> result = await _companyService.UpdateAsync(companyDto);
            return _responseHandler.HandleResult(result);
        }

        [Authorize(Policy = "ActiveUser")]
        [HttpGet]
        public async Task<IActionResult> GetAllCompaniesAsync()
        {
            Result<IEnumerable<RetrieveCompanyDto>> result = await _companyService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        [Authorize(Policy = "ActiveUser")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> DelteCompanyAsync(long id)
        {
            Result result = await _companyService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}
