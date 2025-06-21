using NERBABO.ApiService.Core.Companies.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Companies.Services
{
    public interface ICompanyService
        : IGenericService<RetrieveCompanyDto, CreateCompanyDto, UpdateCompanyDto, long>
    {

    }
}
