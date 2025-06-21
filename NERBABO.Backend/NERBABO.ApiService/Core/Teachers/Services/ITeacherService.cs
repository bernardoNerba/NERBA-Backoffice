using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Teachers.Services;

public interface ITeacherService: IGenericService<RetrieveTeacherDto, CreateTeacherDto, UpdateTeacherDto, long>
{
    Task<Result<RetrieveTeacherDto>> GetByPersonIdAsync(long personId);
}
