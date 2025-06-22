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
    public class GeneralInfoController(
        IGeneralInfoService generalInfoService,
        IResponseHandler responseHandler) : ControllerBase
    {

        private readonly IGeneralInfoService _generalInfoService = generalInfoService;
        private readonly IResponseHandler _responseHandler = responseHandler;


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetGeneralInfoAsync()
        {
            Result<RetrieveGeneralInfoDto> result = await _generalInfoService.GetGeneralInfoAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGeneralInfoAsync([FromBody] UpdateGeneralInfoDto updateConfig)
        {
            Result result = await _generalInfoService.UpdateGeneralInfoAsync(updateConfig);
            return _responseHandler.HandleResult(result);
        }
    }
}
