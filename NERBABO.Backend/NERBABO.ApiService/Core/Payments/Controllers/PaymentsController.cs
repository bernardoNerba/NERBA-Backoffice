using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Authentication.Services;
using NERBABO.ApiService.Core.Payments.Dtos;
using NERBABO.ApiService.Core.Payments.Services;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Payments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController(
        IPaymentsService paymentsService,
        IResponseHandler responseHandler,
        IJwtService jwtService
    ) : ControllerBase
    {
        private readonly IPaymentsService _paymentsService = paymentsService;
        private readonly IResponseHandler _responseHandler = responseHandler;
        private readonly IJwtService _jwtService = jwtService;

        /// <summary>
        /// Gets all teacher payments associated with a given action.
        /// </summary>
        /// <param name="actionId">The ID of the action to retrieve teacher payments for.</param>
        /// <response code="200">Teacher payments found for the given action. Returns a list of teacher payment objects.</response>
        /// <response code="404">Action not found or no teacher payments exist for the given action.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active or user is not Admin nor FM.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("teachers/{actionId}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllTeacherPaymentsByActionIdAsync(long actionId)
        {
            var result = await _paymentsService.GetAllTeacherPaymentsByActionIdAsync(actionId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Updates teacher payments for a specific module teaching.
        /// </summary>
        /// <param name="dto">The UpdateTeacherPaymentsDto object containing the payment details to be updated.</param>
        /// <response code="200">Teacher payments updated successfully.</response>
        /// <response code="400">Validation error in the provided payment data.</response>
        /// <response code="403">User is not authorized to update payments for this module teaching.</response>
        /// <response code="404">Module teaching not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active or user is not Admin nor FM.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("teachers/")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateTeacherPaymentsAsync(UpdateTeacherPaymentsDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Efetue Autenticação para efetuar esta ação.");

            var isAuthorized = await _jwtService.IsCoordOrAdminOfActionViaMTAsync(dto.ModuleTeachingId, userId);
            if (!isAuthorized)
                throw new UnauthorizedAccessException("Não tem autorização para efetuar esta ação.");
            
            var result = await _paymentsService.UpdateTeacherPaymentsByIdAsync(dto);
            return _responseHandler.HandleResult(result);
        }
    }
}
