using NERBABO.ApiService.Core.People.Dtos;

namespace NERBABO.ApiService.Core.People.Services;

public interface IPeopleService
{
    Task<IEnumerable<RetrievePersonDto>> GetAllPeopleAsync();
    Task<RetrievePersonDto?> GetPersonByIdAsync(long id);
    Task<RetrievePersonDto> CreatePersonAsync(CreatePersonDto person);
    Task<RetrievePersonDto?> UpdatePersonAsync(UpdatePersonDto person);
    Task<bool> DeletePersonAsync(long id);
}
