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
    public class MTEnrollmentController(
        IMTEnrollmentService mtEnrollmentService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly IMTEnrollmentService _mtEnrollmentService = mtEnrollmentService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Gets all MT enrollments.
        /// </summary>
        /// <response code="200">There are MT enrollments in the system. Returns a List of RetrieveMTEnrollmentDto.</response>
        /// <response code="404">There are no MT enrollments in the system.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllMTEnrollmentsAsync()
        {
            Result<IEnumerable<RetrieveMTEnrollmentDto>> result = await _mtEnrollmentService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets the MT enrollment with the given id.
        /// </summary>
        /// <param name="id">The ID of the MT enrollment to be retrieved.</param>
        /// <response code="200">MT enrollment found. Returns a RetrieveMTEnrollmentDto.</response>
        /// <response code="404">MT enrollment not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetMTEnrollmentAsync(long id)
        {
            Result<RetrieveMTEnrollmentDto> result = await _mtEnrollmentService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Creates a new MT enrollment.
        /// </summary>
        /// <param name="enrollment">The CreateMTEnrollmentDto object containing the details of the MT enrollment
        /// to be created.</param>
        /// <response code="201">MT enrollment created successfully. Returns the created MT enrollment details.</response>
        /// <response code="400">Validation error - student already enrolled or modules without teachers.</response>
        /// <response code="404">Action or Student not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost("create")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> CreateMTEnrollmentAsync([FromBody] CreateMTEnrollmentDto enrollment)
        {
            Result<RetrieveMTEnrollmentDto> result = await _mtEnrollmentService.CreateAsync(enrollment);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Updates an existing MT enrollment.
        /// </summary>
        /// <param name="id">The ID of the MT enrollment to be updated.</param>
        /// <param name="enrollment">The UpdateMTEnrollmentDto object containing the updated details of the MT enrollment.</param>
        /// <response code="200">MT enrollment updated successfully. Returns the updated MT enrollment details.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="404">MT enrollment not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("update/{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateMTEnrollmentAsync(long id, [FromBody] UpdateMTEnrollmentDto enrollment)
        {
            if (id != enrollment.Id) return BadRequest("ID mismatch");

            Result<RetrieveMTEnrollmentDto> result = await _mtEnrollmentService.UpdateAsync(enrollment);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Deletes an MT enrollment by its ID.
        /// </summary>
        /// <param name="id">The ID of the MT enrollment to be deleted.</param>
        /// <response code="200">MT enrollment deleted successfully.</response>
        /// <response code="404">MT enrollment not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("delete/{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteMTEnrollmentAsync(long id)
        {
            Result result = await _mtEnrollmentService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}