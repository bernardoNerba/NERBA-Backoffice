using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.People.Services;

public interface IPeopleService
{
    Task<Result<IEnumerable<RetrievePersonDto>>> GetAllPeopleAsync();
    Task<Result<RetrievePersonDto>> GetPersonByIdAsync(long id);
    Task<Result<RetrievePersonDto>> CreatePersonAsync(CreatePersonDto person);
    Task<Result<RetrievePersonDto>> UpdatePersonAsync(UpdatePersonDto person);
    Task<Result> DeletePersonAsync(long id);
}
