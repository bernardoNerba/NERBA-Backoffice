using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Enrollments.Dtos;
using NERBABO.ApiService.Core.Enrollments.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Enrollments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionEnrollmentController(
        IActionEnrollmentService actionEnrollmentService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly IActionEnrollmentService _actionEnrollmentService = actionEnrollmentService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Gets all Action enrollments.
        /// </summary>
        /// <response code="200">There are Action enrollments in the system. Returns a List of RetrieveActionEnrollmentDto.</response>
        /// <response code="404">There are no Action enrollments in the system.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllActionEnrollmentsAsync()
        {
            Result<IEnumerable<RetrieveActionEnrollmentDto>> result = await _actionEnrollmentService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets all Action enrollments for a specific action.
        /// </summary>
        /// <param name="actionId">The ID of the action to retrieve enrollments for.</param>
        /// <response code="200">Action enrollments found for the specified action. Returns a List of RetrieveActionEnrollmentDto.</response>
        /// <response code="404">Action not found or no enrollments found for this action.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("by-action/{actionId:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetActionEnrollmentsByActionIdAsync(long actionId)
        {
            Result<IEnumerable<RetrieveActionEnrollmentDto>> result = await _actionEnrollmentService.GetAllByActionId(actionId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets the Action enrollment with the given id.
        /// </summary>
        /// <param name="id">The ID of the Action enrollment to be retrieved.</param>
        /// <response code="200">Action enrollment found. Returns a RetrieveActionEnrollmentDto.</response>
        /// <response code="404">Action enrollment not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetActionEnrollmentAsync(long id)
        {
            Result<RetrieveActionEnrollmentDto> result = await _actionEnrollmentService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Creates a new Action enrollment with automatic initialization of related entities.
        /// This operation creates the action enrollment and automatically generates:
        /// - Session participations for all sessions in the action's module teachings
        /// - Module avaliations for all modules in the action with initial grade of 0
        /// All operations are performed within a database transaction to ensure data consistency.
        /// </summary>
        /// <param name="enrollment">The CreateActionEnrollmentDto object containing the details of the Action enrollment
        /// to be created.</param>
        /// <response code="201">Action enrollment created successfully with all related entities initialized. Returns the created Action enrollment details.</response>
        /// <response code="400">Validation error - student already enrolled or modules without teachers.</response>
        /// <response code="404">Action or Student not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred during transaction. All changes are rolled back.</response>
        [HttpPost("create")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> CreateActionEnrollmentAsync([FromBody] CreateActionEnrollmentDto enrollment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Efetue autenticação.");

            enrollment.UserId = userId;

            Result<RetrieveActionEnrollmentDto> result = await _actionEnrollmentService.CreateAsync(enrollment);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Deletes an Action enrollment by its ID.
        /// This operation will cascade delete all related entities:
        /// - All session participations for this enrollment
        /// - All module avaliations for this enrollment
        /// </summary>
        /// <param name="id">The ID of the Action enrollment to be deleted.</param>
        /// <response code="200">Action enrollment and all related entities deleted successfully.</response>
        /// <response code="404">Action enrollment not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred during deletion.</response>
        [HttpDelete("delete/{id:long}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteActionEnrollmentAsync(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Efetue autenticação.");

            Result result = await _actionEnrollmentService.DeleteIfCoordenatorAsync(id, userId);
            return _responseHandler.HandleResult(result);
        }
    }
}