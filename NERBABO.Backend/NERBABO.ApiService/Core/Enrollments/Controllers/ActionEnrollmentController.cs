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
        /// Creates a new Action enrollment.
        /// </summary>
        /// <param name="enrollment">The CreateActionEnrollmentDto object containing the details of the Action enrollment
        /// to be created.</param>
        /// <response code="201">Action enrollment created successfully. Returns the created Action enrollment details.</response>
        /// <response code="400">Validation error - student already enrolled or modules without teachers.</response>
        /// <response code="404">Action or Student not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost("create")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> CreateActionEnrollmentAsync([FromBody] CreateActionEnrollmentDto enrollment)
        {
            Result<RetrieveActionEnrollmentDto> result = await _actionEnrollmentService.CreateAsync(enrollment);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Updates an existing Action enrollment.
        /// </summary>
        /// <param name="id">The ID of the Action enrollment to be updated.</param>
        /// <param name="enrollment">The UpdateActionEnrollmentDto object containing the updated details of the Action enrollment.</param>
        /// <response code="200">Action enrollment updated successfully. Returns the updated Action enrollment details.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="404">Action enrollment not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("update/{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateActionEnrollmentAsync(long id, [FromBody] UpdateActionEnrollmentDto enrollment)
        {
            if (id != enrollment.Id) return BadRequest("ID mismatch");

            Result<RetrieveActionEnrollmentDto> result = await _actionEnrollmentService.UpdateAsync(enrollment);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Deletes an Action enrollment by its ID.
        /// </summary>
        /// <param name="id">The ID of the Action enrollment to be deleted.</param>
        /// <response code="200">Action enrollment deleted successfully.</response>
        /// <response code="404">Action enrollment not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("delete/{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteActionEnrollmentAsync(long id)
        {
            Result result = await _actionEnrollmentService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}