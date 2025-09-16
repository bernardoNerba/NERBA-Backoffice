using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Actions.Dtos;
using NERBABO.ApiService.Core.Actions.Services;
using NERBABO.ApiService.Core.Authentication.Services;
using NERBABO.ApiService.Shared.Services;
using System.Security.Claims;

namespace NERBABO.ApiService.Core.Actions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionsController(
        ICourseActionService courseActionService,
        IResponseHandler responseHandler,
        IJwtService jwtService
        ) : ControllerBase
    {
        private readonly ICourseActionService _courseActionService = courseActionService;
        private readonly IResponseHandler _responseHandler = responseHandler;
        private readonly IJwtService _jwtService = jwtService;

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
        /// Gets all the actions associated with a given module ordered by status and creation date descencing.
        /// </summary>
        /// <param name="moduleId">The ID of the module to perform the query on.</param>
        /// <response code="200">There are actions on the system associated with the given module. Returns a List of RetrieveCourseActionDto.</response>
        /// <response code="404">Module with given module Id not found or there are no actions on the system associated with the given module.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("module/{moduleId:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllActionsByModuleId(long moduleId)
        {
            var result = await _courseActionService.GetAllByModuleIdAsync(moduleId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets all the actions associated with a given module ordered by status and creation date descencing.
        /// </summary>
        /// <param name="courseId">The ID of the course to perform the query on.</param>
        /// <response code="200">There are actions on the system associated with the given course. Returns a List of RetrieveCourseActionDto.</response>
        /// <response code="404">Course with given course Id not found or there are no actions on the system associated with the given course.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("course/{courseId:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllActionsByCourseId(long courseId)
        {
            var result = await _courseActionService.GetAllByCourseIdAsync(courseId);
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Efetue Autenticação para efetuar esta ação.");

            createCourseActionDto.CoordenatorId = userId;

            var result = await _courseActionService.CreateAsync(createCourseActionDto);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Deletes a action only if the coordenator of the action did the request.
        /// </summary>
        /// <param name="id">Id of the action that the delete will be perfomed.</param>
        /// <response code="200">Action was deleted successfully.</response>
        /// <response code="403">Request user is not the coordenator of this course action nor admin of the system.</response>
        /// <response code="404">Course not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active or user is not Admin nor FM.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteActionIfCoordenatorAsync(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Efetue Autenticação para efetuar esta ação.");

            var isAuthorized = await _jwtService.IsCoordOrAdminOfActionAsync(id, userId);
            if (!isAuthorized)
                throw new UnauthorizedAccessException("Não tem autorização para efetuar esta ação.");

            var result = await _courseActionService.DeleteAsync(id);
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
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Efetue Autenticação para efetuar esta ação.");

            var isAuthorized = await _jwtService.IsCoordOrAdminOfActionAsync(id, userId);
            if (!isAuthorized)
                throw new UnauthorizedAccessException("Não tem autorização para efetuar esta ação.");

            var result = await _courseActionService.UpdateAsync(updateCourseActionDto);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Changes the status of a action by a given Id.
        /// </summary>
        /// <param name="id">The ID of the action to be updated</param>
        /// <param name="status">The string containg the status new values.</param>
        /// <response code="200">The action was updated successfully.</response>
        /// <response code="400">Validation ERROR when validating Status.</response>
        /// <response code="404">The action with given id does not exist or status doesnt exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or FM or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("{id:long}/status")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> ChangeActionStatusAsync(long id, [FromQuery] string status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Efetue Autenticação para efetuar esta ação.");

            var isAuthorized = await _jwtService.IsCoordOrAdminOfActionAsync(id, userId);
            if (!isAuthorized)
                throw new UnauthorizedAccessException("Não tem autorização para efetuar esta ação.");

            var result = await _courseActionService.ChangeActionStatusAsync(id, status, userId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets all the actions associated with a given collaborator (coordenator) ordered by creation date descending.
        /// </summary>
        /// <param name="coordenatorId">The ID of the collaborator (coordenator) to perform the query on.</param>
        /// <response code="200">There are actions on the system associated with the given collaborator. Returns a List of RetrieveCourseActionDto.</response>
        /// <response code="404">Collaborator with given ID not found or there are no actions on the system associated with the given collaborator.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("coordenator/{coordenatorId}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllActionsByCoordenatorId(string coordenatorId)
        {
            var result = await _courseActionService.GetAllByCoordenatorAsync(coordenatorId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets all the necessary information to display in the action kpis cards.
        /// </summary>
        /// <param name="actionId">The ID of the action to be calculated the kpis on.</param>
        /// <response code="200">There is a action on the system with the given id.</response>
        /// <response code="404">The action with the given id was not found on the system.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("kpis/{actionId}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetActionKpisAsync(long actionId)
        {
            var result = await _courseActionService.GetKpisAsync(actionId);
            return _responseHandler.HandleResult(result);
        }
    }
}
