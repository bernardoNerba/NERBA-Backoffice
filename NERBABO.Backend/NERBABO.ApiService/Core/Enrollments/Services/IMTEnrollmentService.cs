using NERBABO.ApiService.Core.Enrollments.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Enrollments.Services;

public interface IMTEnrollmentService
: IGenericService<RetrieveMTEnrollmentDto, CreateMTEnrollmentDto, UpdateMTEnrollmentDto, long>
{
    
}