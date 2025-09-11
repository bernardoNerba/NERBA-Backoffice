using NERBABO.ApiService.Core.SessionParticipations.Dtos;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.SessionParticipations.Services;

public interface ISessionParticipationService
{
    Task<Result<IEnumerable<RetrieveSessionParticipationDto>>> GetBySessionIdAsync(long sessionId);
    Task<Result<IEnumerable<RetrieveSessionParticipationDto>>> GetByActionIdAsync(long actionId);
    Task<Result<IEnumerable<RetrieveSessionParticipationDto>>> UpsertSessionAttendanceAsync(UpsertSessionAttendanceDto upsertDto);
}