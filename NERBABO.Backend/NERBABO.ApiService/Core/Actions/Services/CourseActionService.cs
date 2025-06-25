using NERBABO.ApiService.Core.Actions.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Actions.Services
{
    public class CourseActionService : ICourseActionService
    {
        public Task<Result<RetrieveCourseActionDto>> CreateAsync(CreateCourseActionDto entityDto)
        {
            throw new NotImplementedException();
        }

        public Task<Result> DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<RetrieveCourseActionDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<RetrieveCourseActionDto>> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<RetrieveCourseActionDto>> UpdateAsync(UpdateCourseActionDto entityDto)
        {
            throw new NotImplementedException();
        }
    }
}
