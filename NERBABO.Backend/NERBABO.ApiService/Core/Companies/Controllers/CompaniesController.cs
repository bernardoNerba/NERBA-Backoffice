using Microsoft.AspNetCore.Authorization;
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
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly ILogger<CompaniesController> _logger;
        private readonly IResponseHandler _responseHandler;

        public CompaniesController(
            ICompanyService companyService,
            ILogger<CompaniesController> logger,
            IResponseHandler responseHandler
            )
        {
            _companyService = companyService;
            _logger = logger;
            _responseHandler = responseHandler;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCompanyAsync([FromBody] CreateCompanyDto companyDto)
        {
            Result<RetrieveCompanyDto> result = await _companyService.CreateCompanyAsync(companyDto);
            return _responseHandler.HandleResult(result);
        }

        [Authorize]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetCompanyByIdAsync(long id)
        {
            Result<RetrieveCompanyDto> result = await _companyService.GetCompanyAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}
