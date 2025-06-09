using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Global.Services;

public interface ITaxService
{
    Task<Result<IEnumerable<RetrieveTaxDto>>> GetAllTaxesAsync();
    Task<Result> UpdateTaxAsync(UpdateTaxDto updateTax);
    Task<Result> CreateTaxAsync(CreateTaxDto createTax);
    Task<Result> DeleteTaxAsync(int id);
    Task<Result<IEnumerable<RetrieveTaxDto>>> GetTaxesByTypeAsync(string type);
}
