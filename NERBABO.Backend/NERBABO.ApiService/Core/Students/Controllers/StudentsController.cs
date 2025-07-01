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

    /// <summary>
    /// Get student by student id
    /// </summary>
    /// <param name="id">The student id</param>
    /// <response code="200">The student was found.</response>
    /// <response code="404">The student was not found.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("{id:long}")]
    [Authorize(Policy = "ActiveUser")]
    [ProducesResponseType(typeof(Result<RetrieveStudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<RetrieveStudentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudentByIdAsync(long id)
    {
        Result<RetrieveStudentDto> result = await _studentService.GetByIdAsync(id);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Gets a list of students related to a company
    /// </summary>
    /// <param name="id">The company id</param>
    /// <response code="200">There are students related to the company.</response>
    /// <response code="404">The company was not found or There are no students related to the company.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("company/{id:long}")]
    [Authorize(Policy = "ActiveUser")]
    [ProducesResponseType(typeof(Result<IEnumerable<RetrieveStudentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<IEnumerable<RetrieveStudentDto>>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudentsByCompanyIdAsync(long id)
    {
        Result<IEnumerable<RetrieveStudentDto>> result = await _studentService.GetByCompanyIdAsync(id);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Get student by person id
    /// </summary>
    /// <param name="personId">The person id associated with the student</param>
    /// <response code="200">The student was found.</response>
    /// <response code="401">The user is not authorized. jwt maybe inválid, 
    /// or user does not have one of the roles 'Admin', 'CQ' nor 'FM'.
    /// or the user is not active.</response>
    /// <response code="404">No student was found for the provided person id.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpGet("person/{personId:long}")]
    [Authorize(Policy = "ActiveUser")]
    [ProducesResponseType(typeof(Result<RetrieveStudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<RetrieveStudentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStudentByPersonIdAsync(long personId)
    {
        Result<RetrieveStudentDto> result = await _studentService.GetByPersonIdAsync(personId);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    /// <param name="studentDto">The student data to create</param>
    /// <response code="201">The student was created successfully.</response>
    /// <response code="401">The user is not authorized. jwt maybe inválid, 
    /// or user does not have one of the roles 'Admin', 'CQ' nor 'FM'.
    /// or the user is not active.</response>
    /// <response code="404">The associated person or company was not found.</response>
    /// <response code="404">A student is already associated with the provided person.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpPost]
    [Authorize(Roles = "Admin, CQ, FM", Policy = "ActiveUser")]
    [ProducesResponseType(typeof(Result<RetrieveStudentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result<RetrieveStudentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateStudentAsync([FromBody] CreateStudentDto studentDto)
    {
        Result<RetrieveStudentDto> result = await _studentService.CreateAsync(studentDto);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Update an existing student
    /// </summary>
    /// <param name="id">The student id</param>
    /// <param name="studentDto">The updated student data</param>
    /// <response code="200">The student was updated successfully.</response>
    /// <response code="400">The provided student id does not match the DTO id.</response>
    /// <response code="401">The user is not authorized. jwt maybe inválid, 
    /// or user does not have one of the roles 'Admin', 'CQ' nor 'FM'.
    /// or the user is not active.</response>
    /// <response code="404">The student, associated person, or company was not found.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpPut("{id:long}")]
    [Authorize(Roles = "Admin, CQ, FM", Policy = "ActiveUser")]
    [ProducesResponseType(typeof(Result<RetrieveStudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result<RetrieveStudentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStudentAsync(long id, [FromBody] UpdateStudentDto studentDto)
    {
        if (id != studentDto.Id)
            return BadRequest("ID Mismatch.");

        Result<RetrieveStudentDto> result = await _studentService.UpdateAsync(studentDto);
        return _responseHandler.HandleResult(result);
    }

    /// <summary>
    /// Delete a student by id
    /// </summary>
    /// <param name="id">The student id</param>
    /// <response code="200">The student was deleted successfully.</response>
    /// <response code="401">The user is not authorized. jwt maybe inválid, 
    /// or user does not have one of the roles 'Admin', 'CQ' nor 'FM'.
    /// or the user is not active.</response>
    /// <response code="404">The student was not found.</response>
    /// <response code="500">Unexpected error occurred.</response>
    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin, CQ, FM", Policy = "ActiveUser")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteStudentAsync(long id)
    {
        Result result = await _studentService.DeleteAsync(id);
        return _responseHandler.HandleResult(result);
    }
}