using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Teachers.Services;

public interface ITeacherService
{
    Task<Result<RetrieveTeacherDto>> CreateTeacherAsync(CreateTeacherDto createTeacherDto);
    Task<Result<RetrieveTeacherDto>> GetTeacherByPersonIdAsync(long personId);
    Task<Result<RetrieveTeacherDto>> UpdateTeacherAsync(UpdateTeacherDto updateTeacherDto);
    Task<Result> DeleteTeacherAsync(long teacherId);

}
