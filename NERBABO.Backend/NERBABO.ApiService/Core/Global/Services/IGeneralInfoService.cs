using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Global.Services;

public interface IGeneralInfoService
{
    Task<Result<RetrieveGeneralInfoDto>> GetGeneralInfoAsync();
    Task<Result> UpdateGeneralInfoAsync(UpdateGeneralInfoDto updateGeneralInfo);
    void Dispose();
}
