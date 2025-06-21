using NERBABO.ApiService.Core.Courses.Dtos;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Courses.Services
{
    public interface ICourseService
        : IGenericService<RetrieveCourseDto, CreateCourseDto, UpdateCourseDto, long>
    {
    }
}
