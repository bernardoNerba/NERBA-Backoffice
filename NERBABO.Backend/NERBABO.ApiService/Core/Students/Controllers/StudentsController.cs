using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Students.Dtos;
using NERBABO.ApiService.Core.Students.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Students.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController(
        IStudentService studentService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly IStudentService _studentService = studentService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        [Authorize]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetStudentByIdAsync(long id)
        {
            Result<RetrieveStudentDto> result = await _studentService.GetStudentByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateStudentAsync([FromBody] CreateStudentDto studentDto)
        {
            Result<RetrieveStudentDto> result = await _studentService.CreateStudentAsync(studentDto);
            return _responseHandler.HandleResult(result);
        }
    }
}
