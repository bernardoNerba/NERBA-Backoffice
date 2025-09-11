using NERBABO.ApiService.Core.ModuleAvaliations.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.ModuleAvaliations.Services;

public interface IModuleAvaliationsService
{
    Task<Result<IEnumerable<AvaliationsByModuleDto>>> GetByActionIdAsync(long actionId);
    Task<Result<RetrieveModuleAvaliationDto>> UpdateAsync(UpdateModuleAvaliationDto dto);
}
