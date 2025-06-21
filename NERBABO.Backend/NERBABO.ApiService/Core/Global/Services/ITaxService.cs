using NERBABO.ApiService.Core.Global.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Global.Services;

public interface ITaxService
    : IGenericService<RetrieveTaxDto, CreateTaxDto, UpdateTaxDto, int>
{
    Task<Result<IEnumerable<RetrieveTaxDto>>> GetByTypeAndIsActiveAsync(string type);
}
