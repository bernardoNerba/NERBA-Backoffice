using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Students.Dtos;
using NERBABO.ApiService.Core.Students.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Students.Controllers;

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
        Result<RetrieveStudentDto> result = await _studentService.GetByIdAsync(id);
        return _responseHandler.HandleResult(result);
    }

    [Authorize]
    [HttpGet("person/{personId:long}")]
    public async Task<IActionResult> GetStudentByPersonIdAsync(long personId)
    {
        Result<RetrieveStudentDto> result = await _studentService.GetByPersonIdAsync(personId);
        return _responseHandler.HandleResult(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateStudentAsync([FromBody] CreateStudentDto studentDto)
    {
        Result<RetrieveStudentDto> result = await _studentService.CreateAsync(studentDto);
        return _responseHandler.HandleResult(result);
    }

    [Authorize]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteStudentAsync(long id)
    {
        Result result = await _studentService.DeleteAsync(id);
        return _responseHandler.HandleResult(result);
    }

    [Authorize]
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateStudentAsync(long id, [FromBody] UpdateStudentDto studentDto)
    {
        if (id != studentDto.Id) return BadRequest("ID Missmatch.");
        Result<RetrieveStudentDto> result = await _studentService.UpdateAsync(studentDto);
        return _responseHandler.HandleResult(result);
    }
}
