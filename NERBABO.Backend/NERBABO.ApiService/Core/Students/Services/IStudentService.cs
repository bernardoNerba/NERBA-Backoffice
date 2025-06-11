using NERBABO.ApiService.Core.Students.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Students.Services
{
    public interface IStudentService
    {
        Task<Result<RetrieveStudentDto>> GetStudentByIdAsync(long id);
        Task<Result<RetrieveStudentDto>> GetStudentByPersonIdAsync(long personId);
        Task<Result<RetrieveStudentDto>> CreateStudentAsync(CreateStudentDto studentDto);
        Task<Result<RetrieveStudentDto>> UpdateStudentAsync(UpdateStudentDto studentDto);
        Task<Result> DeleteStudentAsync(long id);
    }
}
