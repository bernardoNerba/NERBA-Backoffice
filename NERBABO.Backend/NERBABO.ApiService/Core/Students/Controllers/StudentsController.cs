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

    [HttpGet("{id:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetStudentByIdAsync(long id)
    {
        Result<RetrieveStudentDto> result = await _studentService.GetByIdAsync(id);
        return _responseHandler.HandleResult(result);
    }

    [HttpGet("person/{personId:long}")]
    [Authorize(Policy = "ActiveUser")]
    public async Task<IActionResult> GetStudentByPersonIdAsync(long personId)
    {
        Result<RetrieveStudentDto> result = await _studentService.GetByPersonIdAsync(personId);
        return _responseHandler.HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin, CQ, FM", Policy = "ActiveUser")]
    public async Task<IActionResult> CreateStudentAsync([FromBody] CreateStudentDto studentDto)
    {
        Result<RetrieveStudentDto> result = await _studentService.CreateAsync(studentDto);
        return _responseHandler.HandleResult(result);
    }

    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin, CQ, FM", Policy = "ActiveUser")]
    public async Task<IActionResult> UpdateStudentAsync(long id, [FromBody] UpdateStudentDto studentDto)
    {
        if (id != studentDto.Id)
            return BadRequest("ID Missmatch.");
        
        Result<RetrieveStudentDto> result = await _studentService.UpdateAsync(studentDto);
        return _responseHandler.HandleResult(result);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin, CQ, FM", Policy = "ActiveUser")]
    public async Task<IActionResult> DeleteStudentAsync(long id)
    {
        Result result = await _studentService.DeleteAsync(id);
        return _responseHandler.HandleResult(result);
    }
}
