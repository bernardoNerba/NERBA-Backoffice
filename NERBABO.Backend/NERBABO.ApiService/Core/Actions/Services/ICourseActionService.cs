
using NERBABO.ApiService.Core.Actions.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Actions.Services
{
    public interface ICourseActionService
        : IGenericService<RetrieveCourseActionDto, CreateCourseActionDto, UpdateCourseActionDto, long>
    {
        Task<Result> DeleteIfCoordenatorAsync(long id, string userId);
        Task<Result<IEnumerable<RetrieveCourseActionDto>>> GetAllByModuleIdAsync(long moduleId);
        Task<Result<IEnumerable<RetrieveCourseActionDto>>> GetAllByCourseIdAsync(long courseId);
        Task<Result> ChangeActionStatusAsync(long id, string status);
    }
}
