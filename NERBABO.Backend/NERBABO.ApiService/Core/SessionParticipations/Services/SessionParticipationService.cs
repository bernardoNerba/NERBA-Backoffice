using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.SessionParticipations.Dtos;
using NERBABO.ApiService.Core.SessionParticipations.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.SessionParticipations.Services;

public class SessionParticipationService(
    AppDbContext context,
    ILogger<SessionParticipationService> logger
) : ISessionParticipationService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<SessionParticipationService> _logger = logger;

    public async Task<Result<IEnumerable<RetrieveSessionParticipationDto>>> GetBySessionIdAsync(long sessionId)
    {
        try
        {
            var participations = await _context.SessionParticipations
                .Include(sp => sp.Session)
                .Include(sp => sp.ActionEnrollment)
                    .ThenInclude(ae => ae.Student)
                        .ThenInclude(s => s.Person)
                .Where(sp => sp.SessionId == sessionId)
                .ToListAsync();

            var retrieveDtos = participations.Select(SessionParticipation.ConvertEntityToRetrieveDto);
            return Result<IEnumerable<RetrieveSessionParticipationDto>>.Ok(retrieveDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session participations by session ID {SessionId}", sessionId);
            return Result<IEnumerable<RetrieveSessionParticipationDto>>
                .Fail("Erro interno.", "Erro ao obter participações da sessão", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RetrieveSessionParticipationDto>>> GetByActionIdAsync(long actionId)
    {
        try
        {
            var participations = await _context.SessionParticipations
                .Include(sp => sp.Session)
                    .ThenInclude(s => s.ModuleTeaching)
                        .ThenInclude(mt => mt.Action)
                .Include(sp => sp.ActionEnrollment)
                    .ThenInclude(ae => ae.Student)
                        .ThenInclude(s => s.Person)
                .Where(sp => sp.Session.ModuleTeaching.Action.Id == actionId)
                .ToListAsync();

            var retrieveDtos = participations.Select(SessionParticipation.ConvertEntityToRetrieveDto);
            return Result<IEnumerable<RetrieveSessionParticipationDto>>.Ok(retrieveDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session participations by action ID {ActionId}", actionId);
            return Result<IEnumerable<RetrieveSessionParticipationDto>>
                .Fail("Erro interno.", "Erro ao obter participações da ação", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RetrieveSessionParticipationDto>>> UpsertSessionAttendanceAsync(UpsertSessionAttendanceDto upsertDto)
    {
        try
        {
            // Validate session exists
            var session = await _context.Sessions
                .Include(s => s.ModuleTeaching)
                    .ThenInclude(mt => mt.Action)
                .FirstOrDefaultAsync(s => s.Id == upsertDto.SessionId);
            if (session is null)
            {
                _logger.LogWarning("Session not found with ID {SessionId} for attendance upsert", upsertDto.SessionId);
                return Result<IEnumerable<RetrieveSessionParticipationDto>>
                    .Fail("Não encontrado.", "Sessão não encontrada", StatusCodes.Status404NotFound);
            }

            // Validate authorization
            if (session.ModuleTeaching.Action.CoordenatorId != upsertDto.UserId)
            {
                _logger.LogWarning("Not possible to process the action since the request user is not the action coordenator.");
                return Result<IEnumerable<RetrieveSessionParticipationDto>>
                    .Fail("Não pode efetuar esta ação.", "Apenas o coordenador da Ação pode realizar esta ação.",
                    StatusCodes.Status401Unauthorized);
            }

            var results = new List<SessionParticipation>();

            foreach (var studentAttendance in upsertDto.StudentAttendances)
            {
                // Validate presence enum
                if (!EnumHelp.IsValidEnum<PresenceEnum>(studentAttendance.Presence))
                {
                    _logger.LogWarning("Invalid presence value {Presence} for student {StudentName}",
                        studentAttendance.Presence, studentAttendance.StudentName);
                    continue;
                }

                // Check if participation already exists
                var existingParticipation = await _context.SessionParticipations
                    .Include(sp => sp.ActionEnrollment)
                        .ThenInclude(ae => ae.Student)
                            .ThenInclude(s => s.Person)
                    .FirstOrDefaultAsync(sp => sp.SessionId == upsertDto.SessionId &&
                        sp.ActionEnrollmentId == studentAttendance.ActionEnrollmentId);

                if (existingParticipation is not null)
                {
                    // Update existing participation
                    existingParticipation.Presence = studentAttendance.Presence.DehumanizeTo<PresenceEnum>();
                    existingParticipation.Attendance = studentAttendance.Attendance;
                    existingParticipation.UpdatedAt = DateTime.UtcNow;
                    results.Add(existingParticipation);
                }
                else
                {
                    // Create new participation
                    var actionEnrollment = await _context.ActionEnrollments
                        .Include(ae => ae.Student)
                            .ThenInclude(s => s.Person)
                        .FirstOrDefaultAsync(ae => ae.Id == studentAttendance.ActionEnrollmentId);

                    if (actionEnrollment is not null)
                    {
                        var newParticipation = new SessionParticipation
                        {
                            SessionId = upsertDto.SessionId,
                            ActionEnrollmentId = studentAttendance.ActionEnrollmentId,
                            Presence = studentAttendance.Presence.DehumanizeTo<PresenceEnum>(),
                            Attendance = studentAttendance.Attendance,
                            Session = session,
                            ActionEnrollment = actionEnrollment,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.SessionParticipations.Add(newParticipation);
                        results.Add(newParticipation);
                    }
                }
            }

            await _context.SaveChangesAsync();

            var retrieveDtos = results.Select(SessionParticipation.ConvertEntityToRetrieveDto);
            _logger.LogInformation("Successfully upserted attendance for {Count} students in session {SessionId}",
                results.Count, upsertDto.SessionId);

            return Result<IEnumerable<RetrieveSessionParticipationDto>>.Ok(retrieveDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting session attendance for session {SessionId}", upsertDto.SessionId);
            return Result<IEnumerable<RetrieveSessionParticipationDto>>
                .Fail("Erro interno.", "Erro ao processar presenças da sessão", StatusCodes.Status500InternalServerError);
        }
    }

}