using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Sessions.Dtos;
using NERBABO.ApiService.Core.Sessions.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Sessions.Services;

public class SessionService(
    AppDbContext context,
    ILogger<SessionService> logger
) : ISessionService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<SessionService> _logger = logger;

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
            .FirstOrDefaultAsync(mt => mt.Id == entityDto.ModuleTeachingId);
        if (moduleTeaching is null)
        {
            _logger.LogWarning("");
            return Result<RetrieveSessionDto>
                .Fail("Não encontrado.", "Relação entre Formador e Módulo da Ação não encontrada",
                StatusCodes.Status404NotFound);
        }

        var action = moduleTeaching.Action;
        // check the action is active
        if (!action.IsActionActive)
        {
            _logger.LogWarning("");
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "Não é possível marcar uma sessão quando a ação está inativa.");
        }

        // check if all the duration of the course is filled
        if (action.Course.TotalDuration - action.Course.CurrentDuration != 0)
        {
            _logger.LogWarning("");
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "O curso em questão não tem a duração total completa. Altere ou adicione os Módulos do Curso.");
        }

        // check if the week day string is a valid WeekDaysEnum
        if (!EnumHelp.IsValidEnum<WeekDaysEnum>(entityDto.Weekday))
        {
            _logger.LogWarning("");
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "O dia da semana inserido não é válido");
        }

        // convert the week day to the enum correspondent
        // and check if is a valid week day for the action
        var weekDay = entityDto.Weekday.DehumanizeTo<WeekDaysEnum>();
        if (!action.WeekDays.Contains(weekDay))
        {
            _logger.LogWarning("");
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.",
                "O dia da semana inserido não está dentro dos dias da semana admitidos na Ação.");
        }

        // check if the date is a valid date and the date is between action start and end dates
        var validStartDate = DateOnly.TryParse(entityDto.ScheduledDate, out DateOnly date);
        if (!validStartDate || (date < action.StartDate) || (date > action.EndDate))
        {
            _logger.LogWarning("");
            return Result<RetrieveSessionDto>
                .Fail("Erro de Validação.", "Data de Inicio invalida.");
        }

        if (moduleTeaching.ScheduledSessionsTime + entityDto.DurationHours > moduleTeaching.Module.Hours)
        {
            _logger.LogWarning("");
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

    public Task<Result> DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<RetrieveSessionDto>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<RetrieveSessionDto>> GetByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<RetrieveSessionDto>> UpdateAsync(UpdateSessionDto entityDto)
    {
        throw new NotImplementedException();
    }
}