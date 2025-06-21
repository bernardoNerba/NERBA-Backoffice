using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTeacherAsync([FromBody] CreateTeacherDto createTeacherDto)
        {
            Result<RetrieveTeacherDto> result = await _teacherService.CreateAsync(createTeacherDto);
            return _responseHandler.HandleResult(result);
        }

        [Authorize]
        [HttpGet("person/{personId}")]
        public async Task<IActionResult> GetTeacherByPersonIdAsync(long personId)
        {
            Result<RetrieveTeacherDto> result = await _teacherService.GetByPersonIdAsync(personId);
            return _responseHandler.HandleResult(result);
        }

        [Authorize]
        [HttpPut("update/{id:long}")]
        public async Task<IActionResult> UpdateTeacherAsync(long id, [FromBody] UpdateTeacherDto updateTeacherDto)
        {
            if (id != updateTeacherDto.Id)
                return BadRequest("ID missmatch.");
            
            Result<RetrieveTeacherDto> result = await _teacherService.UpdateAsync(updateTeacherDto);
            return _responseHandler.HandleResult(result);
        }

        [Authorize]
        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> DeleteTeacherAsync(long id)
        {
            Result result = await _teacherService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    
    }
}
