using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Global.Services;

public interface IGeneralInfoService
{
    Task<Result<RetrieveGeneralInfoDto>> GetGeneralInfoAsync();
    Task<Result> UpdateGeneralInfoAsync(UpdateGeneralInfoDto updateGeneralInfo);
    Task<Result<object>> HealthCheckAsync();
    Task<Result<object>> AliveAsync();
    Task<Result<object>> ReadyAsync();
    void Dispose();
}
