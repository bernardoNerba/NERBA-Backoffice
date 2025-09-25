using NERBABO.ApiService.Core.Payments.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Payments.Services;

public interface IPaymentsService
{
    Task<Result<IEnumerable<TeacherPaymentsDto>>> GetAllTeacherPaymentsByActionIdAsync(long actionId);
    Task<Result> UpdateTeacherPaymentsByIdAsync(UpdateTeacherPaymentsDto dto);
    Task<Result<IEnumerable<StudentPaymentsDto>>> GetAllStudentPaymentsByActionIdAsync(long actionId);
}   