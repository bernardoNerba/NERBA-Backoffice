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

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreatePersonAsync([FromBody] CreatePersonDto person)
        {
            var result = await _peopleService.CreateAsync(person);
            return _responseHandler.HandleResult(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllPersonsAsync()
        {
            var result = await _peopleService.GetAllAsync();            
            return _responseHandler.HandleResult(result);
        }

        [Authorize]
        [HttpGet("not-user")]
        public async Task<IActionResult> GetPeopleWithoutUserAsync()
        {
            var result = await _peopleService.GetAllWithoutUserAsync();
            return _responseHandler.HandleResult(result);
        }

        [Authorize]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetPersonAsync(long id)
        {
            var result = await _peopleService.GetByIdAsync(id);
            return _responseHandler.HandleResult(result);
        }

        [Authorize]
        [HttpPut("update/{id:long}")]
        public async Task<IActionResult> UpdatePersonAsync(long id, [FromBody] UpdatePersonDto person)
        {
            if (id != person.Id) return BadRequest("ID mismatch");

            var result = await _peopleService.UpdateAsync(person);
            return _responseHandler.HandleResult(result);
        }

        [Authorize]
        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> DeletePersonAsync(long id)
        {
            var result = await _peopleService.DeleteAsync(id);
            return _responseHandler.HandleResult(result);
        }
    }
}
