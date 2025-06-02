using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.People.Services;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.People.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly ILogger<PeopleController> _logger;
        private readonly IPeopleService _peopleService;

        public PeopleController(
            ILogger<PeopleController> logger,
            IPeopleService peopleService)
        {
            _logger = logger;
            _peopleService = peopleService;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult> CreatePersonAsync([FromBody] CreatePersonDto person)
        {
            try
            {
                var newPerson = await _peopleService.CreatePersonAsync(person);

                _logger.LogInformation("Person created successfully.");

                return Ok(new OkMessage(
                    "Pessoa Criada.",
                    $"Foi criada uma pessoa com o nome {newPerson.FullName}.",
                    newPerson
                    ));

            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating person");
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<RetrievePersonDto>>> GetAllPersonsAsync()
        {
            var persons = await _peopleService.GetAllPeopleAsync();
            if (persons == null || !persons.Any())
            {
                _logger.LogWarning("No persons found.");
                return NotFound("Não foram encontradas pessoas no sistema.");
            }
            return Ok(persons);
        }

        [Authorize]
        [HttpGet("{id:long}")]
        public async Task<ActionResult<RetrievePersonDto>> GetPersonAsync(long id)
        {
            try
            {
                var person = await _peopleService.GetPersonByIdAsync(id);
                if (person == null)
                {
                    return NotFound("Pessoa não encontrada.");
                }
                return Ok(person);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting single person");
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPut("update/{id:long}")]
        public async Task<ActionResult<RetrievePersonDto>> UpdatePersonAsync(long id, [FromBody] UpdatePersonDto person)
        {
            if (id != person.Id)
            {
                _logger.LogWarning("The id from the person passed on the body is not the same as the one passed on the url params");
                return BadRequest("ID mismatch");
            }

            try
            {
                var updatedPerson = await _peopleService.UpdatePersonAsync(person);
                if (updatedPerson == null)
                {
                    return NotFound("Pessoa não encontrada.");
                }

                return Ok(new OkMessage(
                    "Pessoa Atualizada.",
                    $"Foi atualizada a pessoa com o nome {updatedPerson.FullName}.",
                    updatedPerson
                    ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating person");
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpDelete("delete/{id:long}")]
        public async Task<ActionResult> DeletePersonAsync(long id)
        {
            if (id <= 0)
            {
                return BadRequest("Id de pessoa inválido");
            }

            try
            {
                var result = await _peopleService.DeletePersonAsync(id);
                if (!result)
                {
                    return NotFound("Pessoa não encontrada.");
                }

                return Ok(new OkMessage()
                {
                    Title = "Pessoa Eliminada",
                    Message = $"Foi eliminada a pessoa com o id {id}",
                    Data = result
                });
            }
            catch (Exception e)
            {
                _logger.LogError("Error while trying to delete person: {e}", e.Message);
                return BadRequest(e.Message);
            }
        }


    }
}
