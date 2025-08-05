using NERBABO.ApiService.Core.Modules.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Modules.Services
{
    public interface IModuleService
        : IGenericService<RetrieveModuleDto, CreateModuleDto, UpdateModuleDto, long>
    {
        Task<Result<IEnumerable<RetrieveModuleDto>>> GetActiveModulesAsync();
        Task<Result> ToggleModuleIsActiveAsync(long id);
        Task<Result<IEnumerable<RetrieveModuleDto>>> GetModulesWithoutTeacherByActionIdAsync(long actionId);
    }
}
