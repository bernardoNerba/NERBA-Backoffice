using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Modules.Services
{
    public interface IModuleService
        : IGenericService<RetrieveModuleDto, CreateModuleDto, UpdateModuleDto>
    {
        Task<Result<IEnumerable<RetrieveModuleDto>>> GetActiveModulesAsync();
    }
}
