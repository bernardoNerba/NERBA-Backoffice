using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Actions.Dtos;
using NERBABO.ApiService.Core.Actions.Services;
using NERBABO.ApiService.Shared.Services;
using System.Security.Claims;

namespace NERBABO.ApiService.Core.Actions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionsController(
        ICourseActionService courseActionService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly ICourseActionService _courseActionService = courseActionService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Gets all the actions.
        /// </summary>
        /// <response code="200">There are actions on the system. Returns a List of RetrieveCourseActionDto.</response>
        /// <response code="404">There are no actions on the system.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllActionsAsync()
        {
            var result = await _courseActionService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Get action with the given id.
        /// </summary>
        /// <param name="id">The ID of the action to be retrieved.</param>
        /// <response code="200">Action found. Returns a RetrieveCourseActionDto.</response>
        /// <response code="404">Action not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetActionByIdAsync(long id)
        {
            var result = await _courseActionService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Creates a new action.
        /// </summary>
        /// <param name="createCourseActionDto">The CreateCourseActionDto object containing the details of the action.
        /// to be created.</param>
        /// <response code="201">Action created successfully. Returns the created action.</response>
        /// <response code="400">Validation ERROR when validating Title, AdministrationCode, Status
        /// Regiment, WeekDays or date formats.</response>
        /// <response code="404">Course or coordenator not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active or user is not Admin nor FM.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> CreateActionAsync([FromBody] CreateCourseActionDto createCourseActionDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized("Efetue autenticação.");
            }

            createCourseActionDto.CoordenatorId = userId;

            var result = await _courseActionService.CreateAsync(createCourseActionDto);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Deletes a action only if the coordenator of the action did the request.
        /// </summary>
        /// <param name="id">Id of the action that the delete will be perfomed.</param>
        /// <response code="200">Action was deleted successfully.</response>
        /// <response code="403">Request user is not the coordenator of this course action.</response>
        /// <response code="404">Course not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active or user is not Admin nor FM.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteActionIfCoordenatorAsync(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Unauthorized("Efetue autenticação.");
            }

            var result = await _courseActionService.DeleteIfCoordenatorAsync(id, userId);
            return _responseHandler.HandleResult(result);
        }

        
        /// <summary>
        /// Updates a existing action by a given Id.
        /// </summary>
        /// <param name="id">Id of the action that the update will be perfomed.</param>
        /// <param name="updateCourseActionDto">The UpdateCourseAction object containing the details of the action.
        /// to be updated.</param>
        /// <response code="200">Action updated successfully. Returns the created action.</response>
        /// <response code="400">Validation ERROR when validating Title, AdministrationCode, Status
        /// Regiment, WeekDays, date formats or ID Missmatch.</response>
        /// <response code="404">Course or coordenator not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active or user is not Admin nor FM.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateActionAsync(long id, UpdateCourseActionDto updateCourseActionDto)
        {
            if (updateCourseActionDto.Id != id)
                return BadRequest("ID Missmatch");

            var result = await _courseActionService.UpdateAsync(updateCourseActionDto);
            return _responseHandler.HandleResult(result);
            
        }
    }
}
