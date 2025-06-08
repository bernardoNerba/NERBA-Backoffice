using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public async Task<IActionResult> CreateTeacherAsync([FromBody] CreateTeacherDto createTeacherDto)
        {
            var newTeacher = await _teacherService.CreateTeacherAsync(createTeacherDto);
                
            _logger.LogInformation("Teacher created successfully.");
                
            return Ok(new OkMessage(
                "Formador Criado.",
                $"Foi criado um formador com sucesso.",
                newTeacher
            ));
        }

        [Authorize]
        [HttpGet("person/{personId}")]
        public async Task<ActionResult<RetrieveTeacherDto>> GetTeacherByPersonIdAsync(long personId)
        {
            var teacher = await _teacherService.GetTeacherByPersonIdAsync(personId);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher not found for Person ID: {PersonId}", personId);
                return NotFound("Formador não encontrado para o ID de pessoa fornecido.");
            }
            return Ok(teacher);
        }

        [Authorize]
        [HttpPut("update/{id:long}")]
        public async Task<ActionResult> UpdateTeacherAsync(long id, [FromBody] UpdateTeacherDto updateTeacherDto)
        {
            if (id != updateTeacherDto.Id)
            {
                return BadRequest("ID missmatch.");
            }
            var updatedTeacher = await _teacherService.UpdateTeacherAsync(updateTeacherDto);

            _logger.LogInformation("Teacher updated successfully.");

            return Ok(new OkMessage(
                "Formador Atualizado.",
                $"Foi atualizado o formador com sucesso.",
                updatedTeacher
            ));
        }

        [Authorize]
        [HttpDelete("delete/{id:long}")]
        public async Task<ActionResult> DeleteTeacherAsync(long id)
        {
            var result = await _teacherService.DeleteTeacherAsync(id);

            _logger.LogInformation("Teacher created successfully.");

            return Ok(new OkMessage(
                "Formador Eliminado.",
                $"Foi eliminado um formador com sucesso.",
                result
            ));
        }
    
    }
}
