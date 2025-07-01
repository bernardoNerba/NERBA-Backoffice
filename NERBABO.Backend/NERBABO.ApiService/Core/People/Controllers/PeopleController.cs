using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.People.Services;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.People.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController(
        IPeopleService peopleService,
        IResponseHandler responseHandler
        ) : ControllerBase
    {
        private readonly IPeopleService _peopleService = peopleService;
        private readonly IResponseHandler _responseHandler = responseHandler;

        /// <summary>
        /// Gets all the people.
        /// </summary>
        /// <response code="200">There are people on the system. Returns a List of RetrievePersonDto.</response>
        /// <response code="404">There are no people on the system.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllPersonsAsync()
        {
            Result<IEnumerable<RetrievePersonDto>> result = await _peopleService.GetAllAsync();            
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets all the people that are not associated with a user.
        /// </summary>
        /// <response code="200">There are people that are not associated with a user on the system. Returns a List of RetrievePersonDto.</response>
        /// <response code="404">There are no people that are not associated with a user on the system.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("not-user")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetPeopleWithoutUserAsync()
        {
            Result<IEnumerable<RetrievePersonDto>> result = await _peopleService.GetAllWithoutUserAsync();
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Gets the person with the given id.
        /// </summary>
        /// <param name="id">The ID of the person to be retrieved.</param>
        /// <response code="200">Person found. Returns a RetrievePersonDto.</response>
        /// <response code="404">Person not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpGet("{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetPersonAsync(long id)
        {
            Result<RetrievePersonDto> result = await _peopleService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Creates a new person.
        /// </summary>
        /// <param name="person">The CreatePersonDto object containing the details of the person
        /// to be created.</param>
        /// <response code="201">Person created successfully. Returns the created person details.</response>
        /// <response code="400">Validation error when validating 
        /// NIF, NISS, IdentificationNumber or Email.</response>
        /// <response code="404">Person, Gender, Habilitation, IdentificationType not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPost("create")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> CreatePersonAsync([FromBody] CreatePersonDto person)
        {
            Result<RetrievePersonDto> result = await _peopleService.CreateAsync(person);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Updates an existing person.
        /// </summary>
        /// <param name="id">The ID of the person to be updated.</param>
        /// <param name="person">The UpdatePersonDto object containing the updated details of the person.</param>
        /// <response code="200">Person updated successfully. Returns the created person details.</response>
        /// <response code="400">Validation error when validating 
        /// NIF, NISS, IdentificationNumber or Email.</response>
        /// <response code="404">Person, Gender, Habilitation, IdentificationType not found.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpPut("update/{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> UpdatePersonAsync(long id, [FromBody] UpdatePersonDto person)
        {
            if (id != person.Id) return BadRequest("ID mismatch");

            Result<RetrievePersonDto> result = await _peopleService.UpdateAsync(person);
            return _responseHandler.HandleResult(result);
        }

        /// <summary>
        /// Deletes a person by its ID.
        /// </summary>
        /// <param name="id">The ID of the person to be deleted.</param>
        /// <response code="200">Person deleted successfully.</response>
        /// <response code="404">Person not found.</response>
        /// <response code="400">Person is associated with a user.</response>
        /// <response code="401">Unauthorized access. Invalid jwt, user is not active.</response>
        /// <response code="500">Unexpected error occurred.</response>
        [HttpDelete("delete/{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> DeletePersonAsync(long id)
        {
            Result result = await _peopleService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}
