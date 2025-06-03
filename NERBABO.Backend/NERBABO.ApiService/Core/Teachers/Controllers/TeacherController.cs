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
            catch (KeyNotFoundException e)
            {
                _logger.LogWarning(e, "Key not found while creating teacher");
                return NotFound(e.Message);
            }
            catch (ArgumentException e)
            {
                _logger.LogWarning(e, "Argument exception while creating teacher");
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning(e, "Invalid operation while creating teacher");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while creating teacher.");
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
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Person not found for ID: {PersonId}", personId);
                return NotFound("Pessoa não encontrada.");
            }
            catch (InvalidOperationException)
            {
                _logger.LogWarning("Failed to filter teacher by Person ID: {PersonId}", personId);
                return BadRequest("Falha ao filtrar Formador. Verifique se a pessoa existe e se está associada a um formador.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while filtering teacher by person id.");
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

                _logger.LogInformation("Teacher updated successfully.");

                return Ok(new OkMessage(
                    "Formador Atualizado.",
                    $"Foi atualizado o formador com sucesso.",
                    updatedTeacher
                ));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogWarning(e, "Key not found while updating teacher");
                return NotFound(e.Message);
            }
            catch (ArgumentException e)
            {
                _logger.LogWarning(e, "Argument exception while updating teacher");
                return BadRequest(e.Message);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning(e, "Invalid operation while updating teacher");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while updating teacher.");
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpDelete("delete/{id:long}")]
        public async Task<ActionResult> DeleteTeacherAsync(long id)
        {
            try
            {
                var result = await _teacherService.DeleteTeacherAsync(id);

                _logger.LogInformation("Teacher created successfully.");

                return Ok(new OkMessage(
                    "Formador Eliminado.",
                    $"Foi eliminado um formador com sucesso.",
                    result
                ));
            } 
            catch (KeyNotFoundException e)
            {
                _logger.LogWarning(e, "Key not found to perform query.");
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while deleting teacher.");
                return BadRequest(e.Message);
            }
        }
    
    }
}
