using NERBABO.ApiService.Core.Enrollments.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Enrollments.Services;

public interface IActionEnrollmentService
: IGenericService<RetrieveActionEnrollmentDto, CreateActionEnrollmentDto, UpdateActionEnrollmentDto, long>
{
    Task<Result<IEnumerable<RetrieveActionEnrollmentDto>>> GetAllByActionId(long actionId);
}