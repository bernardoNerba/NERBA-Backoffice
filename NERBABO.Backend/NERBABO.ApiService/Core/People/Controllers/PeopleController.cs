using Microsoft.AspNetCore.Authorization;
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
            catch (InvalidOperationException e)
            {
                _logger.LogWarning(e, "Invalid operation while creating person");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error creating person");
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RetrievePersonDto>>> GetAllPersonsAsync()
        {
            try
            {
                var persons = await _peopleService.GetAllPeopleAsync();

                if (!persons.Any())
                {
                    _logger.LogWarning("No persons found.");
                    return NotFound("Não foram encontradas pessoas no sistema.");
                }

                _logger.LogInformation("Fetching all persons.");
                return Ok(persons);
            }
            catch (ArgumentNullException e)
            {
                _logger.LogWarning(e, "Argument null exception while fetching all persons");
                return NotFound("Não foram encontradas pessoas no sistema.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error logging fetching all persons");
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpGet("{id:long}")]
        public async Task<ActionResult<RetrievePersonDto>> GetPersonAsync(long id)
        {
            try
            {
                var person = await _peopleService.GetPersonByIdAsync(id);

                _logger.LogInformation("Fetching person with ID: {Id}", id);
                return Ok(person);
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogWarning(e, "Person not found for ID: {Id}", id);
                return NotFound("Pessoa não encontrada.");
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
                return BadRequest("ID mismatch");

            try
            {
                var updatedPerson = await _peopleService.UpdatePersonAsync(person);

                _logger.LogInformation("Person updated successfully with ID: {Id}", id);

                return Ok(new OkMessage(
                    "Pessoa Atualizada.",
                    $"Foi atualizada a pessoa com o nome {updatedPerson.FullName}.",
                    updatedPerson
                    ));
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogWarning(e, "Person not found for ID: {Id}", id);
                return NotFound(e.Message);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning(e, "Invalid operation while updating person");
                return BadRequest(e.Message);
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
            try
            {
                await _peopleService.DeletePersonAsync(id);

                _logger.LogInformation("Person deleted successfully with ID: {Id}", id);

                return Ok(new OkMessage()
                {
                    Title = "Pessoa Eliminada",
                    Message = $"Foi eliminada a pessoa com o id {id}",
                    Data = null
                });
            }
            catch (KeyNotFoundException e)
            {
                _logger.LogWarning("Person not found for ID: {Id}", id);
                return NotFound(e.Message);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning("Invalid operation while deleting person: {e}", e.Message);
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Error while trying to delete person: {e}", e.Message);
                return BadRequest(e.Message);
            }
        }


    }
}
