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

    public async Task<Result<RetrieveSessionParticipationDto>> CreateAsync(CreateSessionParticipationDto entityDto)
    {
        // Check if session exists
        var session = await _context.Sessions
            .Include(s => s.ModuleTeaching)
                .ThenInclude(mt => mt.Action)
            .FirstOrDefaultAsync(s => s.Id == entityDto.SessionId);
        if (session is null)
        {
            _logger.LogWarning("Session not found with ID {SessionId} for participation creation", entityDto.SessionId);
            return Result<RetrieveSessionParticipationDto>
                .Fail("Não encontrado.", "Sessão não encontrada",
                StatusCodes.Status404NotFound);
        }

        // Check if action enrollment exists
        var actionEnrollment = await _context.ActionEnrollments
            .Include(ae => ae.Student)
                .ThenInclude(s => s.Person)
            .FirstOrDefaultAsync(ae => ae.Id == entityDto.ActionEnrollmentId
                && ae.ActionId == session.ModuleTeaching.Action.Id);
        if (actionEnrollment is null)
        {
            _logger.LogWarning("ActionEnrollment not found with ID {ActionEnrollmentId} for session {SessionId}", 
                entityDto.ActionEnrollmentId, entityDto.SessionId);
            return Result<RetrieveSessionParticipationDto>
                .Fail("Não encontrado.", "Inscrição do formando não encontrada nesta ação", StatusCodes.Status404NotFound);
        }

        // Check if participation already exists
        var existingParticipation = await _context.SessionParticipations
            .FirstOrDefaultAsync(sp => sp.SessionId == entityDto.SessionId
                && sp.ActionEnrollmentId == entityDto.ActionEnrollmentId);
        if (existingParticipation is not null)
        {
            _logger.LogWarning("SessionParticipation already exists for Session {SessionId} and ActionEnrollment {ActionEnrollmentId}",
                entityDto.SessionId, entityDto.ActionEnrollmentId);
            return Result<RetrieveSessionParticipationDto>
                .Fail("Conflito.", "Participação já existe para esta sessão e formando", StatusCodes.Status409Conflict);
        }

        // Validate presence enum
        if (!EnumHelp.IsValidEnum<PresenceEnum>(entityDto.Presence))
        {
            _logger.LogWarning("Invalid presence value {Presence} for session participation", entityDto.Presence);
            return Result<RetrieveSessionParticipationDto>
                .Fail("Dados inválidos.", "Valor de presença inválido", StatusCodes.Status400BadRequest);
        }

        // Create new session participation
        var newParticipation = new SessionParticipation
        {
            SessionId = entityDto.SessionId,
            ActionEnrollmentId = entityDto.ActionEnrollmentId,
            Presence = entityDto.Presence.DehumanizeTo<PresenceEnum>(),
            Attendance = entityDto.Attendance,
            Session = session,
            ActionEnrollment = actionEnrollment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SessionParticipations.Add(newParticipation);

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("SessionParticipation created successfully with ID {Id}", newParticipation.Id);
            return Result<RetrieveSessionParticipationDto>
                .Ok(SessionParticipation.ConvertEntityToRetrieveDto(newParticipation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session participation");
            return Result<RetrieveSessionParticipationDto>
                .Fail("Erro interno.", "Erro ao criar participação na sessão", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<RetrieveSessionParticipationDto>> UpdateAsync(UpdateSessionParticipationDto entityDto)
    {
        var existingParticipation = await _context.SessionParticipations
            .Include(sp => sp.Session)
            .Include(sp => sp.ActionEnrollment)
                .ThenInclude(ae => ae.Student)
                    .ThenInclude(s => s.Person)
            .FirstOrDefaultAsync(sp => sp.Id == entityDto.SessionParticipationId);

        if (existingParticipation is null)
        {
            _logger.LogWarning("SessionParticipation not found with ID {SessionParticipationId} for update", entityDto.SessionParticipationId);
            return Result<RetrieveSessionParticipationDto>
                .Fail("Não encontrado.", "Participação na sessão não encontrada", StatusCodes.Status404NotFound);
        }

        // Validate presence enum
        if (!EnumHelp.IsValidEnum<PresenceEnum>(entityDto.Presence))
        {
            _logger.LogWarning("Invalid presence value {Presence} for session participation update", entityDto.Presence);
            return Result<RetrieveSessionParticipationDto>
                .Fail("Dados inválidos.", "Valor de presença inválido", StatusCodes.Status400BadRequest);
        }

        // Update participation
        existingParticipation.Presence = entityDto.Presence.DehumanizeTo<PresenceEnum>();
        existingParticipation.Attendance = entityDto.Attendance;
        existingParticipation.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("SessionParticipation updated successfully with ID {Id}", existingParticipation.Id);
            return Result<RetrieveSessionParticipationDto>
                .Ok(SessionParticipation.ConvertEntityToRetrieveDto(existingParticipation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session participation");
            return Result<RetrieveSessionParticipationDto>
                .Fail("Erro interno.", "Erro ao atualizar participação na sessão", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RetrieveSessionParticipationDto>>> GetAllAsync()
    {
        try
        {
            var participations = await _context.SessionParticipations
                .Include(sp => sp.Session)
                .Include(sp => sp.ActionEnrollment)
                    .ThenInclude(ae => ae.Student)
                        .ThenInclude(s => s.Person)
                .ToListAsync();

            var retrieveDtos = participations.Select(SessionParticipation.ConvertEntityToRetrieveDto);
            return Result<IEnumerable<RetrieveSessionParticipationDto>>.Ok(retrieveDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all session participations");
            return Result<IEnumerable<RetrieveSessionParticipationDto>>
                .Fail("Erro interno.", "Erro ao obter participações nas sessões", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<RetrieveSessionParticipationDto>> GetByIdAsync(long id)
    {
        try
        {
            var participation = await _context.SessionParticipations
                .Include(sp => sp.Session)
                .Include(sp => sp.ActionEnrollment)
                    .ThenInclude(ae => ae.Student)
                        .ThenInclude(s => s.Person)
                .FirstOrDefaultAsync(sp => sp.Id == id);

            if (participation is null)
            {
                _logger.LogWarning("SessionParticipation not found with ID {Id}", id);
                return Result<RetrieveSessionParticipationDto>
                    .Fail("Não encontrado.", "Participação na sessão não encontrada", StatusCodes.Status404NotFound);
            }

            return Result<RetrieveSessionParticipationDto>
                .Ok(SessionParticipation.ConvertEntityToRetrieveDto(participation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session participation by ID {Id}", id);
            return Result<RetrieveSessionParticipationDto>
                .Fail("Erro interno.", "Erro ao obter participação na sessão", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        try
        {
            var participation = await _context.SessionParticipations.FindAsync(id);
            if (participation is null)
            {
                _logger.LogWarning("SessionParticipation not found with ID {Id} for deletion", id);
                return Result.Fail("Não encontrado.", "Participação na sessão não encontrada", StatusCodes.Status404NotFound);
            }

            _context.SessionParticipations.Remove(participation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("SessionParticipation deleted successfully with ID {Id}", id);
            return Result.Ok("Sucesso.", "Participação na sessão eliminada com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting session participation with ID {Id}", id);
            return Result.Fail("Erro interno.", "Erro ao eliminar participação na sessão", StatusCodes.Status500InternalServerError);
        }
    }

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