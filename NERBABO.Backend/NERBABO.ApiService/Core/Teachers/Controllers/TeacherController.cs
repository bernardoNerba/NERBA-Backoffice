using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Core.Teachers.Services;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Teachers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly ILogger<TeacherController> _logger;
        public TeacherController(
            ITeacherService teacherService,
            ILogger<TeacherController> logger)
        {
            _teacherService = teacherService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateTeacherAsync([FromBody] CreateTeacherDto createTeacherDto)
        {
            try
            {
                var newTeacher = await _teacherService.CreateTeacherAsync(createTeacherDto);
                _logger.LogInformation("Teacher created successfully.");
                return Ok(new OkMessage(
                    "Formador Criado.",
                    $"Foi criado um formador com sucesso.",
                    newTeacher
                ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating teacher");
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpGet("person/{personId}")]
        public async Task<ActionResult<RetrieveTeacherDto>> GetTeacherByPersonIdAsync(long personId)
        {
            try
            {
                var teacher = await _teacherService.GetTeacherByPersonIdAsync(personId);
                if (teacher == null)
                {
                    _logger.LogWarning("Teacher not found for Person ID: {PersonId}", personId);
                    return NotFound("Formador não encontrado para o ID de pessoa fornecido.");
                }
                return Ok(teacher);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving teacher by person ID");
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPut("update/{id:long}")]
        public async Task<ActionResult> UpdateTeacherAsync(long id, [FromBody] UpdateTeacherDto updateTeacherDto)
        {
            if (id != updateTeacherDto.Id)
            {
                return BadRequest("ID missmatch.");
            }
            try
            {
                var updatedTeacher = await _teacherService.UpdateTeacherAsync(updateTeacherDto);
                return Ok(new OkMessage(
                    "Formador Atualizado.",
                    $"Foi atualizado o formador com sucesso.",
                    updatedTeacher
                ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating teacher");
                return BadRequest(e.Message);
            }
        }
    }
}
