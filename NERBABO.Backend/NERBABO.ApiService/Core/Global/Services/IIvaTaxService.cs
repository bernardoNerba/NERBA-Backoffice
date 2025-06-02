using NERBABO.ApiService.Core.Global.Dtos;

namespace NERBABO.ApiService.Core.Global.Services;

public interface IIvaTaxService
{
    Task<IEnumerable<RetrieveIvaTaxDto>> GetAllIvaTaxesAsync();
    Task UpdateIvaTaxAsync(UpdateIvaTaxDto updateIvaTax);
    Task CreateTaxIvaAsync(CreateIvaTaxDto createIvaTax);
    Task DeleteTaxIvaAsync(int id);
}
