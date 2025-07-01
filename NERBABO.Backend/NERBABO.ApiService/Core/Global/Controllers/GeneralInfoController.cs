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

        /// <summary>
        /// Retrieves general information.
        /// </summary>
        /// <response code="200">General information retrieved successfully. Returns RetrieveGeneralInfoDto.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetGeneralInfoAsync()
        {
            Result<RetrieveGeneralInfoDto> result = await _generalInfoService.GetGeneralInfoAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Updates general information.
        /// </summary>
        /// <param name="updateConfig">The UpdateGeneralInfoDto object that will be validated and used to update the general information.</param>
        /// <response code="200">Updated Successfully. Returns the updated RetrieveGeneralInfoDto.</response>
        /// <response code="400">Validation ERROR when validating UpdateGeneralInfoDto.</response>
        /// <response code="404">Iva tax with given ivaId not found.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGeneralInfoAsync([FromBody] UpdateGeneralInfoDto updateConfig)
        {
            Result result = await _generalInfoService.UpdateGeneralInfoAsync(updateConfig);
            return _responseHandler.HandleResult(result);
        }
    }
}
