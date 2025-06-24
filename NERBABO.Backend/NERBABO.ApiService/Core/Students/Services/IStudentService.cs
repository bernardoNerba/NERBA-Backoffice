using NERBABO.ApiService.Core.Students.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Students.Services
{
    public interface IStudentService: IGenericService<RetrieveStudentDto, CreateStudentDto, UpdateStudentDto, long>
    {
        Task<Result<RetrieveStudentDto>> GetByPersonIdAsync(long personId);
        Task<Result<IEnumerable<RetrieveStudentDto>>> GetByCompanyIdAsync(long companyId);
    }
}
