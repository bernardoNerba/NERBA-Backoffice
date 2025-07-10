using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Courses.Dtos;
using NERBABO.ApiService.Core.Courses.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Courses.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController(
        ICourseService courseService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly ICourseService _courseService = courseService;
        private readonly IResponseHandler _responseHandler = responseHandler;


        /// <summary>
        /// Get all courses
        /// </summary>
        /// <response code="200">There are courses on the system. Returns a list of all courses.</response>
        /// <response code="404">There are no courses on the system.</response>
        /// <response code="401">The user is not authorized, invalid jwt or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllCoursesAsync()
        {
            var result = await _courseService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Get all active courses
        /// </summary>
        /// <response code="200">There are active courses on the system. Returns a list
        /// of all active courses.</response>
        /// <response code="404">There are no active courses on the system.</response>
        /// <response code="401">The user is not authorized, invalid jwt or user is
        /// not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("active")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllActiveCoursesAsync()
        {
            var result = await _courseService.GetAllActiveAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Get all courses by frame ID
        /// </summary>
        /// <param name="frameId">The ID of the frame to filter courses by.</param>
        /// <response code="200">There are courses associated with the given frame ID. Returns a list of courses.</response>
        /// <response code="404">There are no courses associated with the given frame ID or there is no frame with the given frameId</response>
        /// <response code="401">The user is not authorized, invalid jwt or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("frame/{frameId:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllCoursesByFrameIdAsync(long frameId)
        {
            var result = await _courseService.GetAllByFrameIdAsync(frameId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Get all courses by module ID
        /// </summary>
        /// <param name="moduleId">The ID of the module to filter courses by.</param>
        /// <response code="200">There are courses associated with the given module ID. Returns a list of courses.</response>
        /// <response code="404">There are no courses associated with the given module ID or there is no module with the given moduleId</response>
        /// <response code="401">The user is not authorized, invalid jwt or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("module/{moduleId:long}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> GetCoursesByModuleAsync(long moduleId)
        {
            var result = await _courseService.GetCoursesByModuleIdAsync(moduleId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Get a course by its ID
        /// </summary>
        /// <param name="id">The ID of the course to retrieve.</param>
        /// <response code="200">The course with the given ID exists. Returns the course
        /// details.</response>
        /// <response code="404">The course with the given ID does not exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetCourseByIdAsync(long id)
        {
            var result = await _courseService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Create a new course
        /// </summary>
        /// <param name="createCourseDto">The DTO containing the details of the course to be created.</param>
        /// <response code="201">The course was created successfully. Returns the created course details.</response>
        /// <response code="400">Validation ERROR when validating course title, Habilitation Level, Status or Destinators.</response>
        /// <response code="404">The frame with given frameId doesnt exists.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or FM or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> CreateCourseAsync([FromBody] CreateCourseDto createCourseDto)
        {
            var result = await _courseService.CreateAsync(createCourseDto);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Update a course by its ID
        /// </summary>
        /// <param name="id">The ID of the course to be updated.</param>
        /// <param name="updateCourseDto">The DTO containing the updated details of the course to be updated.</param>
        /// <response code="200">The course was updated successfully. Returns the updated course details</response>
        /// <response code="400">Validation ERROR when validating course title, Habilitation Level, Status or Destinators.</response>
        /// <response code="404">The frame or course with given id does not exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or FM or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> UpdateCourseAsync(long id, [FromBody] UpdateCourseDto updateCourseDto)
        {
            if (id != updateCourseDto.Id)
                return BadRequest("ID Missmatch");

            var result = await _courseService.UpdateAsync(updateCourseDto);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Update a course status by its ID
        /// </summary>
        /// <param name="id">The ID of the course to be updated.</param>
        /// <param name="status">The string containing the status new values.</param>
        /// <response code="200">The course was updated successfully.</response>
        /// <response code="400">Validation ERROR when validating Status.</response>
        /// <response code="404">The course with given id does not exist or status doesnt exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or FM or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("{id:long}/status")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> ChangeCourseStatusAsync(long id, [FromQuery] string status)
        {
            var result = await _courseService.ChangeCourseStatusAsync(id, status);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Delete a course by its ID
        /// </summary>
        /// <param name="id">The ID of the course to be deleted.</param>
        /// <response code="200">The course was deleted successfully.</response>
        /// <response code="404">The course with the given ID does not exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> DeleteCourseAsync(long id)
        {
            var result = await _courseService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Assign a module to a course
        /// </summary>
        /// <param name="courseId">The ID of the course to which the module will
        /// be assigned.</param>
        /// <param name="moduleId">The ID of the module to be assigned to the
        /// course.</param>
        /// <response code="200">The module was assigned to the course successfully. Returns the updated course details.</response>
        /// <response code="400">Validation ERROR the module must be active in order to be assigned,
        /// the course must be active in order to assign a module to it.
        /// The module is already assigned to the course or
        /// the course total duration will exceed if the module is added.</response>
        /// <response code="404">The course or module with the given ID does not exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or FM or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost("{courseId:long}/module/{moduleId:long}")]
        [Authorize(Roles = "Admin, FM", Policy = "ActiveUser")]
        public async Task<IActionResult> AssignModuleToCourseAsync(long courseId, long moduleId)
        {
            Result<RetrieveCourseDto> result = await _courseService.AssignModuleAsync(moduleId, courseId);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Unassign a module from a course
        /// </summary>
        /// <param name="courseId">The ID of the course from which the module will
        /// be unassigned.</param>
        /// <param name="moduleId">The ID of the module to be unassigned from the
        /// course.</param>
        /// <response code="200">The module was unassigned from the course successfully. Returns the updated course details.</response>
        /// <response code="400">Validation ERROR the course must be active in order to be unassigned a module.</response>
        /// <response code="404">The course or module with the given ID does not exist.</response>
        /// <response code="401">The user is not authorized, invalid jwt, user is not Admin or FM or user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost("{courseId:long}/module/{moduleId:long}/unassign")]
        [Authorize(Roles = "Admin", Policy = "ActiveUser")]
        public async Task<IActionResult> UnassignModuleToCourseAsync(long courseId, long moduleId)
        {
            Result<RetrieveCourseDto> result = await _courseService.UnassignModuleAsync(moduleId, courseId);
            return _responseHandler.HandleResult(result);
        }
    }
}
