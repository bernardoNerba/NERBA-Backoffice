using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Modules.Services
{
    public interface IModuleService
    {
        Task<Result<IEnumerable<Module>>> GetAllModulesAsync();
        Task<Result<Module>> GetModuleByIdAsync(long id);
        Task<Result<Module>> CreateModuleAsync(CreateModuleDto moduleDto);
        Task<Result<Module>> UpdateModuleAsync(UpdateModuleDto moduleDto);
        Task<Result> DeleteModuleAsync(long id);
        Task<Result<IEnumerable<Module>>> GetActiveModulesAsync();
    }
}
