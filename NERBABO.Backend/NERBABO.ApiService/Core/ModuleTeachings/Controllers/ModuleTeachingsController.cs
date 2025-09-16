using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.ModuleTeachings.Dtos;
using NERBABO.ApiService.Core.ModuleTeachings.Services;
using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;
using NERBABO.ApiService.Core.Sessions.Dtos;
using System.Security.Claims;
using NERBABO.ApiService.Core.Authentication.Services;

namespace NERBABO.ApiService.Core.ModuleTeachings.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModuleTeachingsController(
        IModuleTeachingService moduleTeachingService,
        IModuleService moduleService,
        IResponseHandler responseHandler,
        IJwtService jwtService
        ) : ControllerBase
    {
        private readonly IModuleTeachingService _moduleTeachingService = moduleTeachingService;
        private readonly IModuleService _moduleService = moduleService;
        private readonly IResponseHandler _responseHandler = responseHandler;
        private readonly IJwtService _jwtService = jwtService;

        /// <summary>
        /// Gets all the module teachings (teacher-module-action associations).
        /// </summary>
        /// <response code="200">There are module teachings on the system. Returns a List of RetrieveModuleTeachingDto.</response>
        /// <response code="404">There are no module teachings on the system.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllModuleTeachingsAsync()
        {
            Result<IEnumerable<RetrieveModuleTeachingDto>> result = await _moduleTeachingService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets the module teaching with the given id.
        /// </summary>
        /// <param name="id">The ID of the module teaching to be retrieved.</param>
        /// <response code="200">Module teaching found. Returns a RetrieveModuleTeachingDto.</response>
        /// <response code="404">Module teaching not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetModuleTeachingAsync(long id)
        {
            Result<RetrieveModuleTeachingDto> result = await _moduleTeachingService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Creates a new module teaching association between a teacher, module, and action.
        /// </summary>
        /// <param name="moduleTeaching">The CreateModuleTeachingDto object containing the details of the association
        /// to be created.</param>
        /// <response code="201">Module teaching created successfully. Returns the created association details.</response>
        /// <response code="400">Validation error. Module not associated with action's course, or teacher already assigned to this module in this action.</response>
        /// <response code="404">Teacher, Module, or Action not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active or doesnt have role Admin nor FM. Is not Action coordenator.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost("create")]
        [Authorize(Policy = "ActiveUser", Roles = "Admin, FM")]
        public async Task<IActionResult> CreateModuleTeachingAsync([FromBody] CreateModuleTeachingDto moduleTeaching)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Efetue Autenticação para efetuar esta ação.");

            var isAuthorized = await _jwtService.IsCoordOrAdminOfActionAsync(moduleTeaching.ActionId, userId);
            if (!isAuthorized)
                throw new UnauthorizedAccessException("Não tem autorização para efetuar esta ação.");

            Result<RetrieveModuleTeachingDto> result = await _moduleTeachingService.CreateAsync(moduleTeaching);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Updates an existing module teaching association.
        /// </summary>
        /// <param name="id">The ID of the module teaching to be updated.</param>
        /// <param name="moduleTeaching">The UpdateModuleTeachingDto object containing the updated details of the association.</param>
        /// <response code="200">Module teaching updated successfully. Returns the updated association details.</response>
        /// <response code="400">Validation error. ID mismatch, no changes detected, module not associated with action's course, or another teacher already assigned to this module in this action.</response>
        /// <response code="404">Module teaching, Teacher, Module, or Action not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active or doesnt have role Admin nor FM. Is not Action coordenator.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("update/{id:long}")]
        [Authorize(Policy = "ActiveUser", Roles = "Admin, FM")]
        public async Task<IActionResult> UpdateModuleTeachingAsync(long id, [FromBody] UpdateModuleTeachingDto moduleTeaching)
        {
            if (id != moduleTeaching.Id) return BadRequest("ID mismatch");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Efetue Autenticação para efetuar esta ação.");

            var isAuthorized = await _jwtService.IsCoordOrAdminOfActionAsync(moduleTeaching.ActionId, userId);
            if (!isAuthorized)
                throw new UnauthorizedAccessException("Não tem autorização para efetuar esta ação.");

            Result<RetrieveModuleTeachingDto> result = await _moduleTeachingService.UpdateAsync(moduleTeaching);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Deletes a module teaching association by its ID.
        /// </summary>
        /// <param name="id">The ID of the module teaching to be deleted.</param>
        /// <response code="200">Module teaching deleted successfully.</response>
        /// <response code="404">Module teaching not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active or doesnt have role Admin nor FM. Is not Action coordenator.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("delete/{id:long}")]
        [Authorize(Policy = "ActiveUser", Roles = "Admin, FM")]
        public async Task<IActionResult> DeleteModuleTeachingAsync(long id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Efetue Autenticação para efetuar esta ação.");

            var isAuthorized = await _jwtService.IsCoordOrAdminOfActionViaMTAsync(id, userId);
            if (!isAuthorized)
                throw new UnauthorizedAccessException("Não tem autorização para efetuar esta ação.");

            Result result = await _moduleTeachingService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets all modules from an action's course that don't have teachers assigned.
        /// </summary>
        /// <param name="actionId">The ID of the action to check for unassigned modules.</param>
        /// <response code="200">Modules without teachers found. Returns a List of RetrieveModuleDto.</response>
        /// <response code="404">Action not found or all modules already have teachers assigned.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("action/{actionId:long}/modules-without-teacher")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetModulesWithoutTeacherByActionAsync(long actionId)
        {
            Result<IEnumerable<RetrieveModuleDto>> result = await _moduleService.GetModulesWithoutTeacherByActionIdAsync(actionId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets the module teaching association for a specific action and module.
        /// </summary>
        /// <param name="actionId">The ID of the action.</param>
        /// <param name="moduleId">The ID of the module.</param>
        /// <response code="200">ModuleTeaching association found. Returns the association details.</response>
        /// <response code="404">ModuleTeaching association not found for this action and module.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("action/{actionId:long}/module/{moduleId:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetModuleTeachingByActionAndModuleAsync(long actionId, long moduleId)
        {
            Result<RetrieveModuleTeachingDto> result = await _moduleTeachingService.GetByActionAndModuleAsync(actionId, moduleId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets all module teaching association for a specific action in a minimal way.
        /// </summary>
        /// <param name="actionId">The ID of the action.</param>
        /// <response code="200">ModuleTeachings associations found. Returns all the association details.</response>
        /// <response code="404">ModuleTeachings associations not found for this action.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("action/{actionId:long}/")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetModuleTeachingByActionAndModuleAsync(long actionId)
        {
            Result<IEnumerable<MinimalModuleTeachingDto>> result = await _moduleTeachingService.GetByActionIdMinimalAsync(actionId);
            return _responseHandler.HandleResult(result);
        }


        /// <summary>
        /// Gets all module teaching payment details for a specific action.
        /// </summary>
        /// <param name="actionId">The ID of the action to retrieve payment details for.</param>
        /// <response code="200">Module teaching payments found. Returns a List of ProcessModuleTeachingPaymentDto.</response>
        /// <response code="404">Action not found or no module teaching payments available for this action.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active or doesnt have role Admin nor FM.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("mt-payments-by-action/{actionId}")]
        [Authorize(Policy = "ActiveUser", Roles = "Admin, FM")]
        public async Task<IActionResult> GetAllModuleTeachingPaymentsAsync(long actionId)
        {
            Result<IEnumerable<ProcessModuleTeachingPaymentDto>> result = await _moduleTeachingService.GetAllModuleTeachingPaymentAsync(actionId);
            return _responseHandler.HandleResult(result);
        }
    }
}