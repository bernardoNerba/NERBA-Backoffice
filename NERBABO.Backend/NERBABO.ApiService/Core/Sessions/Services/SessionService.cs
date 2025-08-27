using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Sessions.Dtos;
using NERBABO.ApiService.Core.Sessions.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.Sessions.Services;

public class SessionService(
    AppDbContext context,
    ILogger<SessionService> logger,
    UserManager<User> userManager
) : ISessionService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<SessionService> _logger = logger;
    private readonly UserManager<User> _userManager = userManager;

    public async Task<Result<RetrieveSessionDto>> CreateAsync(CreateSessionDto entityDto)
    {
        // check if the relation between the Teacher and Module of a Action exists
        var moduleTeaching = await _context.ModuleTeachings
            .Include(mt => mt.Sessions)
            .Include(mt => mt.Teacher)
                .ThenInclude(mt => mt.Person)
            .Include(mt => mt.Module)
            .Include(mt => mt.Action)
                .ThenInclude(a => a.Course)
                    .ThenInclude(c => c.Modules)
            .Include(mt => mt.Action.Coordenator)
                .ThenInclude(c => c.Person)
            .FirstOrDefaultAsync(mt => mt.Id == entityDto.ModuleTeachingId);
        if (moduleTeaching is null)
        {
            _logger.LogWarning("ModuleTeaching not found with ID {ModuleTeachingId} for session creation by user {UserId}", 
                entityDto.ModuleTeachingId, entityDto.User.Id);
            return Result<RetrieveSessionDto>
                .Fail("Não encontrado.", "Relação entre Formador e Módulo da Ação não encontrada",
                StatusCodes.Status404NotFound);
        }

        var action = moduleTeaching.Action;

        // check if the user that made the request is the coordenator of the action or is admin
        if ((action.CoordenatorId != entityDto.User.Id)
            && !(await _userManager.GetRolesAsync(entityDto.User)).Contains("Admin"))
        {
            _logger.LogWarning("Unauthorized session creation attempt by user {UserId} for action {ActionId}. User is not coordinator or admin", 
                entityDto.User.Id, action.Id);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                    "Apenas o coordenador pode realizar esta ação.",
                    StatusCodes.Status401Unauthorized);
        }

        // check the action is active
        if (!action.IsActionActive)
        {
            _logger.LogWarning("Attempted to create session for inactive action {ActionId} by user {UserId}", 
                action.Id, entityDto.User.Id);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "Não é possível marcar uma sessão quando a ação está inativa.");
        }        

        // check if all the duration of the course is filled
        if (action.Course.TotalDuration - action.Course.CurrentDuration != 0)
        {
            _logger.LogWarning("Course duration validation failed for action {ActionId}. Total: {TotalDuration}, Current: {CurrentDuration}", 
                action.Id, action.Course.TotalDuration, action.Course.CurrentDuration);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "O curso em questão não tem a duração total completa. Altere ou adicione os Módulos do Curso.");
        }

        // check if the week day string is a valid WeekDaysEnum
        if (!EnumHelp.IsValidEnum<WeekDaysEnum>(entityDto.Weekday))
        {
            _logger.LogWarning("Invalid weekday '{Weekday}' provided for session creation by user {UserId}", 
                entityDto.Weekday, entityDto.User.Id);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "O dia da semana inserido não é válido");
        }

        // convert the week day to the enum correspondent
        // and check if is a valid week day for the action
        var weekDay = entityDto.Weekday.DehumanizeTo<WeekDaysEnum>();
        if (!action.WeekDays.Contains(weekDay))
        {
            _logger.LogWarning("Weekday '{Weekday}' is not allowed for action {ActionId}. Allowed days: {AllowedDays}", 
                entityDto.Weekday, action.Id, string.Join(", ", action.WeekDays));
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "O dia da semana inserido não está dentro dos dias da semana admitidos na Ação.");
        }

        // check if the date is a valid date and the date is between action start and end dates
        var validStartDate = DateOnly.TryParse(entityDto.ScheduledDate, out DateOnly date);
        if (!validStartDate || (date < action.StartDate) || (date > action.EndDate))
        {
            _logger.LogWarning("Invalid scheduled date '{ScheduledDate}' for action {ActionId}. Action period: {StartDate} to {EndDate}", 
                entityDto.ScheduledDate, action.Id, action.StartDate, action.EndDate);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.", "Data de Inicio invalida.");
        }

        // check if the start time is a valid TimeOnly
        var validStartTime = TimeOnly.TryParse(entityDto.Start, out TimeOnly startTime);
        if (!validStartTime)
        {
            _logger.LogWarning("Invalid start time '{StartTime}' provided for session creation by user {UserId}", 
                entityDto.Start, entityDto.User.Id);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.", "Hora de início inválida.");
        }

        if (moduleTeaching.ScheduledSessionsTime + entityDto.DurationHours > moduleTeaching.Module.Hours)
        {
            _logger.LogWarning("Duration validation failed for ModuleTeaching {ModuleTeachingId}. Current: {CurrentHours}, Adding: {AddingHours}, Module Total: {ModuleHours}", 
                moduleTeaching.Id, moduleTeaching.ScheduledSessionsTime, entityDto.DurationHours, moduleTeaching.Module.Hours);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "Adicionar as horas desta sessão ultrapassaria o máximo de horas do módulo.");
        }
        
        var createdEntity = _context.Sessions.Add(Session.ConvertCreateDtoToEntity(entityDto, moduleTeaching));
            await _context.SaveChangesAsync();

            var retrieveSession = Session.ConvertEntityToRetrieveDto(createdEntity.Entity);

            _logger.LogInformation("Session created successfully for ModuleTeaching {ModuleTeachingId} on {ScheduledDate}.", entityDto.ModuleTeachingId, entityDto.ScheduledDate);

            return Result<RetrieveSessionDto>
                .Ok(retrieveSession, "Sessão criada.", 
                "A sessão foi agendada com sucesso.");

    }

    public async Task<Result> DeleteIfActionCoordenatorAsync(long id, User user)
    {
        // TODO: Handle relationships deletes
        var existingSession = await _context.Sessions
            .Include(s => s.ModuleTeaching)
                .ThenInclude(mt => mt.Action)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (existingSession is null)
        {
            _logger.LogWarning("Session with ID {SessionId} not found for deletion by user {UserId}", id, user.Id);
            return Result
            .Fail("Não encontrado", "Sessão não encontrada.",
            StatusCodes.Status404NotFound);
        }

        if (!(await _userManager.GetRolesAsync(user)).Contains("Admin"))
        {
            if (existingSession.ModuleTeaching.Action.CoordenatorId != user.Id)
            {
                _logger.LogWarning("Unauthorized session deletion attempt by user {UserId} for session {SessionId}. User is not the action coordinator", 
                    user.Id, id);
                return Result
                    .Fail("Erro de Validação.",
                        "Apenas o coordenador do curso pode realizar esta ação.",
                        StatusCodes.Status401Unauthorized);
            }
        }

        // Check if the session was already lecture
        if (existingSession.TeacherPresence.Equals(PresenceEnum.Present))
        {
            _logger.LogWarning("Attempted to delete session {SessionId} that was already taught (teacher present)", id);
            return Result
            .Fail("Erro de Validação", "A sessão já foi lecionada, não é possível eliminar.");
        }

        // Check if is action coordenator

        _context.Sessions.Remove(existingSession);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Session deleted successfully with ID: {SessionId}", id);
        return Result
            .Ok("Sessão eliminada.", "Sessão eliminada com sucesso.");
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var existingSession = await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == id);
        if (existingSession is null)
        {
            _logger.LogWarning("Session with ID {SessionId} not found for deletion", id);
            return Result
            .Fail("Não encontrado", "Sessão não encontrada.",
            StatusCodes.Status404NotFound);
        }

        // Check if the session was already lecture
        if (existingSession.TeacherPresence.Equals(PresenceEnum.Present))
        {
            _logger.LogWarning("Attempted to delete session {SessionId} that was already taught (teacher present)", id);
            return Result
            .Fail("Erro de Validação", "A sessão já foi lecionada, não é possível eliminar.");
        }

        // Check if is action coordenator
        _context.Sessions.Remove(existingSession);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Session deleted successfully with ID: {SessionId}", id);
        return Result
            .Ok("Sessão eliminada.", "Sessão eliminada com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveSessionDto>>> GetAllAsync()
    {
        var existingSessions = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.ModuleTeaching)
            .Include(s => s.ModuleTeaching.Teacher)
                    .ThenInclude(mt => mt.Person)
            .Include(s => s.ModuleTeaching.Module)
            .Include(s => s.ModuleTeaching.Action)
                .ThenInclude(a => a.Course)
                    .ThenInclude(c => c.Modules)
            .Include(s => s.ModuleTeaching.Action.Coordenator)
                .ThenInclude(c => c.Person)
            .OrderByDescending(s => s.ScheduledDate)
            .ToListAsync()
            ?? [];

        var retrieveSessions = existingSessions.Select(Session.ConvertEntityToRetrieveDto);

        _logger.LogInformation("Successfully retrieved {SessionCount} sessions", 
            retrieveSessions.Count());

        return Result<IEnumerable<RetrieveSessionDto>>
            .Ok(retrieveSessions);
    }

    public async Task<Result<IEnumerable<RetrieveSessionDto>>> GetAllByActionIdAsync(long actionId)
    {
        var existingAction = await _context.Actions
            .FirstOrDefaultAsync(a => a.Id == actionId);
        if (existingAction is null)
        {
            _logger.LogWarning("Action with ID {ActionId} not found when retrieving sessions", actionId);
            return Result<IEnumerable<RetrieveSessionDto>>
                .Fail("Não encontrado", "Ação Formação não encontrada");
        }

        var existingSessionsOnAction = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.ModuleTeaching)
            .Include(s => s.ModuleTeaching.Teacher)
                    .ThenInclude(mt => mt.Person)
            .Include(s => s.ModuleTeaching.Module)
            .Include(s => s.ModuleTeaching.Action)
                .ThenInclude(a => a.Course)
                    .ThenInclude(c => c.Modules)
            .Include(s => s.ModuleTeaching.Action.Coordenator)
                .ThenInclude(c => c.Person)
            .Where(s => s.ModuleTeaching.ActionId == actionId)
            .OrderByDescending(s => s.ScheduledDate)
            .ToListAsync()
            ?? [];

        var retrieveSessions = existingSessionsOnAction
            .Select(Session.ConvertEntityToRetrieveDto);

        _logger.LogInformation("Successfully retrieved {SessionCount} sessions for action {ActionId}", 
            retrieveSessions.Count(), actionId);

        return Result<IEnumerable<RetrieveSessionDto>>
            .Ok(retrieveSessions);
    }

    public async Task<Result<RetrieveSessionDto>> GetByIdAsync(long id)
    {

        var existingSession = await _context.Sessions
            .AsNoTracking()
            .Include(s => s.ModuleTeaching)
            .Include(s => s.ModuleTeaching.Teacher)
                    .ThenInclude(mt => mt.Person)
            .Include(s => s.ModuleTeaching.Module)
            .Include(s => s.ModuleTeaching.Action)
                .ThenInclude(a => a.Course)
                    .ThenInclude(c => c.Modules)
            .Include(s => s.ModuleTeaching.Action.Coordenator)
                .ThenInclude(c => c.Person)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (existingSession is null)
        {
            _logger.LogWarning("Session with ID {SessionId} not found", id);
            return Result<RetrieveSessionDto>
                .Fail("Não encontrado.", "Sessão não encontrada.",
                StatusCodes.Status404NotFound);
        }

        _logger.LogInformation("Successfully retrieved session with ID {SessionId}", id);
        var retrieveSession = Session.ConvertEntityToRetrieveDto(existingSession);
        return Result<RetrieveSessionDto>
            .Ok(retrieveSession);
    }

    public async Task<Result<RetrieveSessionDto>> UpdateAsync(UpdateSessionDto entityDto)
    {
        var existingSession = await _context.Sessions
            .Include(s => s.ModuleTeaching)
            .Include(s => s.ModuleTeaching.Teacher)
                    .ThenInclude(mt => mt.Person)
            .Include(s => s.ModuleTeaching.Module)
            .Include(s => s.ModuleTeaching.Action)
                .ThenInclude(a => a.Course)
                    .ThenInclude(c => c.Modules)
            .Include(s => s.ModuleTeaching.Action.Coordenator)
                .ThenInclude(c => c.Person)
            .FirstOrDefaultAsync(s => s.Id == entityDto.Id);
        if (existingSession is null)
        {
            _logger.LogWarning("Session with ID {SessionId} not found for update by user {UserId}", 
                entityDto.Id, entityDto.User.Id);
            return Result<RetrieveSessionDto>
                .Fail("Não encontrado.", "Sessão para atualizar não encontrada.",
                StatusCodes.Status404NotFound);
        }

        var action = existingSession.ModuleTeaching.Action;

        // check if the user that made the request is the coordenator of the action or is admin
        if ((action.CoordenatorId != entityDto.User.Id)
            && !(await _userManager.GetRolesAsync(entityDto.User)).Contains("Admin"))
        {
            _logger.LogWarning("Unauthorized session update attempt by user {UserId} for session {SessionId}. User is not coordinator or admin", 
                entityDto.User.Id, entityDto.Id);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                    "Apenas o coordenador pode realizar esta ação.",
                    StatusCodes.Status401Unauthorized);
        }

        // check if the action is active
        if (!action.IsActionActive)
        {
            _logger.LogWarning("Attempted to update session {SessionId} for inactive action {ActionId} by user {UserId}", 
                entityDto.Id, action.Id, entityDto.User.Id);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "Não é possível modificar uma sessão quando a ação está inativa.");
        }

        // check if all the duration of the course is filled
        if (action.Course.TotalDuration - action.Course.CurrentDuration != 0)
        {
            _logger.LogWarning("Course duration validation failed for action {ActionId} during session {SessionId} update. Total: {TotalDuration}, Current: {CurrentDuration}", 
                action.Id, entityDto.Id, action.Course.TotalDuration, action.Course.CurrentDuration);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "O curso em questão não tem a duração total completa. Altere ou adicione os Módulos do Curso.");
        }

        // check if the week day string is a valid WeekDaysEnum
        if (!EnumHelp.IsValidEnum<WeekDaysEnum>(entityDto.Weekday))
        {
            _logger.LogWarning("Invalid weekday '{Weekday}' provided for session {SessionId} update by user {UserId}", 
                entityDto.Weekday, entityDto.Id, entityDto.User.Id);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "O dia da semana inserido não é válido");
        }

        // convert the week day to the enum correspondent
        // and check if is a valid week day for the action
        var weekDay = entityDto.Weekday.DehumanizeTo<WeekDaysEnum>();
        if (!action.WeekDays.Contains(weekDay))
        {
            _logger.LogWarning("Weekday '{Weekday}' is not allowed for action {ActionId}. Allowed days: {AllowedDays}", 
                entityDto.Weekday, action.Id, string.Join(", ", action.WeekDays));
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "O dia da semana inserido não está dentro dos dias da semana admitidos na Ação.");
        }

        // check if the date is a valid date and the date is between action start and end dates
        var validStartDate = DateOnly.TryParse(entityDto.ScheduledDate, out DateOnly date);
        if (!validStartDate || (date < action.StartDate) || (date > action.EndDate))
        {
            _logger.LogWarning("Invalid scheduled date '{ScheduledDate}' for action {ActionId}. Action period: {StartDate} to {EndDate}", 
                entityDto.ScheduledDate, action.Id, action.StartDate, action.EndDate);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.", "Data de Inicio invalida.");
        }

        // check if the start time is a valid TimeOnly
        var validStartTime = TimeOnly.TryParse(entityDto.Start, out TimeOnly startTime);
        if (!validStartTime)
        {
            _logger.LogWarning("Invalid start time '{StartTime}' provided for session {SessionId} update by user {UserId}", 
                entityDto.Start, entityDto.Id, entityDto.User.Id);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.", "Hora de início inválida.");
        }

        // calculate the total scheduled hours for this module teaching, excluding the current session being updated
        var otherSessionsTime = existingSession.ModuleTeaching.ScheduledSessionsTime - existingSession.DurationHours;
        if (otherSessionsTime + entityDto.DurationHours > existingSession.ModuleTeaching.Module.Hours)
        {
            _logger.LogWarning("Duration validation failed for session {SessionId} update. Other sessions time: {OtherTime}, New duration: {NewDuration}, Module total: {ModuleHours}", 
                entityDto.Id, otherSessionsTime, entityDto.DurationHours, existingSession.ModuleTeaching.Module.Hours);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "Modificar a duração desta sessão ultrapassaria o máximo de horas do módulo.");
        }

        // check if session was already taught (prevent modification if teacher was present)
        // if (existingSession.TeacherPresence.Equals(PresenceEnum.Present))
        // {
        //     _logger.LogWarning("");
        //     return Result<RetrieveSessionDto>
        //         .Fail("Erro de Validação.",
        //         "Não é possível modificar uma sessão que já foi lecionada.");
        // }

        // validate teacher presence enum if provided
        if (!string.IsNullOrEmpty(entityDto.TeacherPresence) 
            && !EnumHelp.IsValidEnum<PresenceEnum>(entityDto.TeacherPresence))
        {
            _logger.LogWarning("Invalid teacher presence '{TeacherPresence}' provided for session {SessionId} update by user {UserId}", 
                entityDto.TeacherPresence, entityDto.Id, entityDto.User.Id);
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "O estado de presença do formador inserido não é válido.");
        }

        // update the session properties
        existingSession.Weekday = weekDay;
        existingSession.ScheduledDate = date;
        existingSession.Start = startTime;
        existingSession.DurationHours = entityDto.DurationHours;
        existingSession.Note = entityDto.Note;
        
        if (!string.IsNullOrEmpty(entityDto.TeacherPresence))
        {
            existingSession.TeacherPresence = entityDto.TeacherPresence.DehumanizeTo<PresenceEnum>();
        }

        existingSession.UpdatedAt = DateTime.UtcNow;

        _context.Sessions.Update(existingSession);
        await _context.SaveChangesAsync();

        var retrieveSession = Session.ConvertEntityToRetrieveDto(existingSession);

        _logger.LogInformation("Session updated successfully with ID: {SessionId}", entityDto.Id);

        return Result<RetrieveSessionDto>
            .Ok(retrieveSession, "Sessão atualizada.", 
            "A sessão foi atualizada com sucesso.");
    }
}