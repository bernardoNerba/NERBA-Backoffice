using NERBABO.ApiService.Core.Companies.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Companies.Services
{
    public interface ICompanyService
    {
        Task<Result<IEnumerable<RetrieveCompanyDto>>> GetAllCompaniesAsync();
        Task<Result<RetrieveCompanyDto>> GetCompanyAsync(long id);
        Task<Result<RetrieveCompanyDto>> CreateCompanyAsync(CreateCompanyDto createCompanyDto);
        Task<Result<RetrieveCompanyDto>> UpdateCompanyAsync(UpdateCompanyDto updateCompanyDto);
        Task<Result> DeleteCompanyAsync(long id);

    }
}
