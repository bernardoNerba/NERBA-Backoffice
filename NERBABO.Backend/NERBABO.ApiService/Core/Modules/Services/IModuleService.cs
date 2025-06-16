using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Modules.Services
{
    public interface IModuleService
    {
        Task<Result<IEnumerable<RetrieveModuleDto>>> GetAllModulesAsync();
        Task<Result<RetrieveModuleDto>> GetModuleByIdAsync(long id);
        Task<Result<RetrieveModuleDto>> CreateModuleAsync(CreateModuleDto moduleDto);
        Task<Result<RetrieveModuleDto>> UpdateModuleAsync(UpdateModuleDto moduleDto);
        Task<Result> DeleteModuleAsync(long id);
        Task<Result<IEnumerable<RetrieveModuleDto>>> GetActiveModulesAsync();
    }
}
