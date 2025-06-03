using NERBABO.ApiService.Core.Global.Dtos;

namespace NERBABO.ApiService.Core.Global.Services;

public interface ITaxService
{
    Task<IEnumerable<RetrieveTaxDto>> GetAllTaxesAsync();
    Task UpdateTaxAsync(UpdateTaxDto updateTax);
    Task CreateTaxAsync(CreateTaxDto createTax);
    Task DeleteTaxAsync(int id);
    Task<IEnumerable<RetrieveTaxDto>> GetTaxesByTypeAsync(string type);
}
