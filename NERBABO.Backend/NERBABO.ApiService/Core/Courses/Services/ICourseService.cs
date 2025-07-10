using NERBABO.ApiService.Core.Courses.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Courses.Services
{
    public interface ICourseService
        : IGenericService<RetrieveCourseDto, CreateCourseDto, UpdateCourseDto, long>
    {
        Task<Result<IEnumerable<RetrieveCourseDto>>> GetAllActiveAsync();
        Task<Result<IEnumerable<RetrieveCourseDto>>> GetAllByFrameIdAsync(long frameId);
        Task<Result<RetrieveCourseDto>> AssignModuleAsync(long moduleId, long courseId);
        Task<Result<RetrieveCourseDto>> UnassignModuleAsync(long moduleId, long courseId);
        Task<Result<IEnumerable<RetrieveCourseDto>>> GetCoursesByModuleIdAsync(long moduleId);
        Task<Result> ChangeCourseStatusAsync(long id, string status);
        Task<Result<RetrieveCourseDto>> UpdateCourseModulesAsync(List<long> moduleIds, long courseId);
    }
}
