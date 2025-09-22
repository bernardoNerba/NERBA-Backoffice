using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Payments.Dtos;
using NERBABO.ApiService.Core.Payments.Services;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Payments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(
        IPaymentsService paymentsService,
        IResponseHandler responseHandler
    ) : ControllerBase
    {
        private readonly IPaymentsService _paymentsService = paymentsService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        // TODO: Verify is user is Admin or Coordenator of the action
        [HttpGet("teachers/{actionId}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllTeacherPaymentsByActionIdAsync(long actionId)
        {
            var result = await _paymentsService.GetAllTeacherPaymentsByActionIdAsync(actionId);
            return _responseHandler.HandleResult(result);
        }

        // TODO: Verify is user is Admin or Coordenator of the action
        [HttpPut("teachers/")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateTeacherPaymentsAsync(UpdateTeacherPaymentsDto dto)
        {
            var result = await _paymentsService.UpdateTeacherPaymentsByIdAsync(dto);
            return _responseHandler.HandleResult(result);
        }
    }
}
