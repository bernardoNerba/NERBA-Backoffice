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

        [HttpGet]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetAllPersonsAsync()
        {
            Result<IEnumerable<RetrievePersonDto>> result = await _peopleService.GetAllAsync();            
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("not-user")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetPeopleWithoutUserAsync()
        {
            Result<IEnumerable<RetrievePersonDto>> result = await _peopleService.GetAllWithoutUserAsync();
            return _responseHandler.HandleResult(result);
        }

        [HttpGet("{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> GetPersonAsync(long id)
        {
            Result<RetrievePersonDto> result = await _peopleService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        [HttpPost("create")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> CreatePersonAsync([FromBody] CreatePersonDto person)
        {
            Result<RetrievePersonDto> result = await _peopleService.CreateAsync(person);
            return _responseHandler.HandleResult(result);
        }

        [HttpPut("update/{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> UpdatePersonAsync(long id, [FromBody] UpdatePersonDto person)
        {
            if (id != person.Id) return BadRequest("ID mismatch");

            Result<RetrievePersonDto> result = await _peopleService.UpdateAsync(person);
            return _responseHandler.HandleResult(result);
        }

        [HttpDelete("delete/{id:long}")]
        [Authorize(Policy = "ActiveUser")]
        public async Task<IActionResult> DeletePersonAsync(long id)
        {
            Result result = await _peopleService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}
