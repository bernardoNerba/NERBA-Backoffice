using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Core.Teachers.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Teachers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController(
        ITeacherService teacherService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly ITeacherService _teacherService = teacherService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Gets the teacher associated with a given id.
        /// </summary>
        /// <param name="id">The teacher id to perform query.</param>
        /// <response code="200">The teacher with the id was found. returns a RetrieveTeacherDto type response.</response>
        /// <response code="404">The teacher with the id not found.</response>
        /// <response code="401">The user is not authorized, invalid jwt or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        /// [Authorize(Policy = "ActiveUser")]
        [Authorize(Policy = "ActiveUser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherByIdAsync(long id)
        {
            Result<RetrieveTeacherDto> result = await _teacherService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets the teacher associated with a person, by the personid.
        /// </summary>
        /// <param name="personId">The person id to perform query.</param>
        /// <response code="200">The person with the personId was found. returns a RetrieveTeacherDto type response.</response>
        /// <response code="404">The person with the personId not found. The person with the personId is not a teacher.</response>
        /// <response code="401">The user is not authorized, invalid jwt or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [Authorize(Policy = "ActiveUser")]
        [HttpGet("person/{personId}")]
        public async Task<IActionResult> GetTeacherByPersonIdAsync(long personId)
        {
            Result<RetrieveTeacherDto> result = await _teacherService.GetByPersonIdAsync(personId);
            return _responseHandler.HandleResult(result);
        }


        /// <summary>
        /// Creates a new Teacher.
        /// </summary>
        /// <param name="createTeacherDto">The teacher object that will be created.</param>
        /// <remarks>
        /// </remarks>
        /// <response code="201">The teacher was created successfully.</response>
        /// <response code="404">The IVA, IRS or person regiment was not found.</response>
        /// <response code="404">The person was not found.</response>
        /// <response code="400">Validation ERROR when validating iva, irs, person unique relationship or ccp must be unique.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user has not the 'Admin', 'CQ', 'FM' role or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [Authorize(Roles = "Admin, CQ, FM", Policy = "ActiveUser")]
        [HttpPost]
        public async Task<IActionResult> CreateTeacherAsync([FromBody] CreateTeacherDto createTeacherDto)
        {
            Result<RetrieveTeacherDto> result = await _teacherService.CreateAsync(createTeacherDto);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Update Teacher info and relationships.
        /// </summary>
        /// <param name="id">Id of the Teacher that will be perfomed the action.</param>
        /// <param name="updateTeacherDto">The UpdateTeacherDto object that will validated and perfomed action.</param>
        /// <remarks>
        /// </remarks>
        /// <response code="200">The teacher was updated successfully.</response>
        /// <response code="400">Id Missmatch</response>
        /// <response code="404">The IVA, IRS, teacher, person was not found.</response>
        /// <response code="400">Validation ERROR when validating iva, irs, person, ccp field.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user has not the 'Admin', 'CQ', 'FM' role or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [Authorize(Roles = "Admin, CQ, FM", Policy = "ActiveUser")]
        [HttpPut("update/{id:long}")]
        public async Task<IActionResult> UpdateTeacherAsync(long id, [FromBody] UpdateTeacherDto updateTeacherDto)
        {
            if (id != updateTeacherDto.Id)
                return BadRequest("ID missmatch.");
            
            Result<RetrieveTeacherDto> result = await _teacherService.UpdateAsync(updateTeacherDto);
            return _responseHandler.HandleResult(result);
        }


        /// <summary>
        /// Delete a teacher by id.
        /// </summary>
        /// <param name="id">Id of the Teacher that will be perfomed the action.</param>
        /// <response code="200">The teacher was deleted successfully.</response>
        /// <response code="404">The teacher to perfom deletion was not found.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user has not the 'Admin', 'CQ', 'FM' role or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        // TODO: Missing some delete validation when future constrains added
        [Authorize(Roles = "Admin, CQ, FM", Policy = "ActiveUser")]
        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> DeleteTeacherAsync(long id)
        {
            Result result = await _teacherService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    
    }
}
