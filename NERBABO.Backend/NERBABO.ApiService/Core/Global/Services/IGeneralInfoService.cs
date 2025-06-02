using NERBABO.ApiService.Core.Global.Dtos;

namespace NERBABO.ApiService.Core.Global.Services;

public interface IGeneralInfoService
{
    Task<RetrieveGeneralInfoDto> GetGeneralInfoAsync();
    Task UpdateGeneralInfoAsync(UpdateGeneralInfoDto updateGeneralInfo);
    void Dispose();
}
