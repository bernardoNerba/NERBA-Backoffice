using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Courses.Dtos;
using NERBABO.ApiService.Core.Courses.Services;
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


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllCoursesAsync()
        {
            var result = await _courseService.GetAllAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("active")]
        [Authorize]
        public async Task<IActionResult> GetAllActiveCoursesAsync()
        {
            var result = await _courseService.GetAllActiveAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("frame/{frameId:long}")]
        [Authorize]
        public async Task<IActionResult> GetAllCoursesByFrameIdAsync(long frameId)
        {
            var result = await _courseService.GetAllByFrameIdAsync(frameId);
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("{id:long}")]
        [Authorize]
        public async Task<IActionResult> GetCourseByIdAsync(long id)
        {
            var result = await _courseService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, FM")]
        public async Task<IActionResult> CreateCourseAsync([FromBody] CreateCourseDto createCourseDto)
        {
            var result = await _courseService.CreateAsync(createCourseDto);
            return _responseHandler.HandleResult(result);
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin, FM")]
        public async Task<IActionResult> UpdateCourseAsync(long id, [FromBody] UpdateCourseDto updateCourseDto)
        {
            if (id != updateCourseDto.Id)
                return BadRequest("ID Missmatch");

            var result = await _courseService.UpdateAsync(updateCourseDto);
            return _responseHandler.HandleResult(result);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin, FM")]
        public async Task<IActionResult> DeleteCourseAsync(long id)
        {
            var result = await _courseService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}
