using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.People.Services;

public interface IPeopleService: IGenericService<RetrievePersonDto, CreatePersonDto, UpdatePersonDto, long>
{
    Task<Result<IEnumerable<RetrievePersonDto>>> GetAllWithoutUserAsync();
}
