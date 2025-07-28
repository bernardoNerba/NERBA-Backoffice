using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Actions.Cache;
using NERBABO.ApiService.Core.Actions.Dtos;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.Actions.Services
{
    public class CourseActionService(
        AppDbContext context,
        ILogger<CourseActionService> logger,
        ICacheActionRepository cache
        ) : ICourseActionService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<CourseActionService> _logger = logger;
        private readonly ICacheActionRepository _cache = cache;

        public async Task<Result<RetrieveCourseActionDto>> CreateAsync(CreateCourseActionDto entityDto)
        {
            var existingCourse = await _context.Courses
                .FindAsync(entityDto.CourseId);
            if (existingCourse is null)
            {
                _logger.LogWarning("Course not found for given CourseId: {id}", entityDto.CourseId);
                return Result<RetrieveCourseActionDto>
                    .Fail("Não encontrado.", "Curso não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            var existingCoordenator = await _context.Users
                .FindAsync(entityDto.CoordenatorId);
            if (existingCoordenator is null)
            {
                _logger.LogWarning("Coordenator not found for given CoordenatorId: {id}", entityDto.CoordenatorId);
                return Result<RetrieveCourseActionDto>
                    .Fail("Não encontrado.", "Coordenador não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            // validate unique constraint
            if (!string.IsNullOrEmpty(entityDto.Title)
                && await _context.Actions.AnyAsync(a =>
                a.Title.ToLower()
                .Equals(entityDto.Title.ToLower())))
            {
                _logger.LogWarning("Action with title '{title}' already exists.", entityDto.Title);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Já existe uma ação com o mesmo título.");
            }

            if (!string.IsNullOrEmpty(entityDto.AdministrationCode)
                && await _context.Actions.AnyAsync(a =>
                a.AdministrationCode.ToLower()
                .Equals(entityDto.AdministrationCode.ToLower()))
                )
            {
                _logger.LogWarning("Action with administration code '{code}' already exists.", entityDto.AdministrationCode);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Já existe uma ação com o mesmo código administrativo.");
            }

            // validate enums
            if (!string.IsNullOrEmpty(entityDto.Status)
                && !EnumHelp.IsValidEnum<StatusEnum>(entityDto.Status))
            {
                _logger.LogWarning("Invalid status value: {status}", entityDto.Status);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Estado inválido.");
            }

            if (!string.IsNullOrEmpty(entityDto.Regiment)
                && !EnumHelp.IsValidEnum<RegimentTypeEnum>(entityDto.Regiment))
            {
                _logger.LogWarning("Invalid regiment value: {regiment}", entityDto.Regiment);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Regimento inválido.");
            }

            foreach (var weekDay in entityDto.WeekDays)
            {
                if (!EnumHelp.IsValidEnum<WeekDaysEnum>(weekDay))
                {
                    _logger.LogWarning("Invalid week day value: {weekDay}", weekDay);
                    return Result<RetrieveCourseActionDto>
                        .Fail("Erro de Validação", "Dia da semana inválido.");
                }
            }

            // Date formats and validation
            if (!DateOnly.TryParse(entityDto.StartDate, out var startDate))
            {
                _logger.LogWarning("Invalid start date format: {startDate}", entityDto.StartDate);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Data de início inválida.");
            }

            if (!DateOnly.TryParse(entityDto.EndDate, out var endDate))
            {
                _logger.LogWarning("Invalid end date format: {endDate}", entityDto.EndDate);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Data de fim inválida.");
            }

            if (startDate >= endDate)
            {
                _logger.LogWarning("Start date {startDate} must be before end date {endDate}.", startDate, endDate);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "A data de início deve ser anterior à data de fim.");
            }

            var action = CourseAction.ConvertCreateDtoToEntity(entityDto, existingCoordenator, existingCourse);
            var result = _context.Actions.Add(action);
            _context.SaveChanges();

            var retrieveCourseAction = CourseAction.ConvertEntityToRetrieveDto(result.Entity, existingCoordenator, existingCourse);

            // Update Cache
            await _cache.RemoveActionCacheAsync();
            await _cache.SetSingleActionCacheAsync(retrieveCourseAction);

            return Result<RetrieveCourseActionDto>
                .Ok(retrieveCourseAction,
                "Ação de Formação criada.", "Ação de Formação criada com sucesso.",
                StatusCodes.Status201Created);
        }

        public async Task<Result> DeleteIfCoordenatorAsync(long id, string userId)
        {
            var existingCourseAction = await _context.Actions
                .Include(a => a.Coordenator)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (existingCourseAction is null)
            {
                _logger.LogWarning("Course action not found for given ID: {id}", id);
                return Result.Fail("Não encontrado.", "Ação não encontrada.",
                    StatusCodes.Status404NotFound);
            }

            if (existingCourseAction.CoordenatorId != userId)
            {
                _logger.LogWarning("User {userId} is not the coordinator of the action with ID {actionId}.", userId, id);
                return Result.Fail("Não autorizado.", "Não é o coordenador desta ação formativa.",
                    StatusCodes.Status403Forbidden);
            }

            await DeleteTransactionAsync(existingCourseAction);

            // update cache
            await _cache.RemoveActionCacheAsync(id);

            return Result
                .Ok("Ação Formativa Eliminada.", "Ação Formativa eliminada com sucesso.");
        }

        public async Task<Result> DeleteAsync(long id)
        {
            // Check if the action exists
            var existingCourseAction = await _context.Actions
                .Include(a => a.Coordenator)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (existingCourseAction is null)
            {
                _logger.LogWarning("Course action not found for given ID: {id}", id);
                return Result.Fail("Não encontrado.", "Ação não encontrada.",
                    StatusCodes.Status404NotFound);
            }

            await DeleteTransactionAsync(existingCourseAction);

            // update cache
            await _cache.RemoveActionCacheAsync(id);

            return Result
            .Ok("Ação Formativa Eliminada.", "Ação Formativa eliminada com sucesso.");
        }

        public async Task<Result<IEnumerable<RetrieveCourseActionDto>>> GetAllAsync()
        {
            var cacheActions = await _cache.GetCacheAllActionsAsync();
            if (cacheActions is not null && cacheActions.ToList().Count != 0)
                return Result<IEnumerable<RetrieveCourseActionDto>>
                    .Ok(cacheActions);

            var existingCourseActions = await _context.Actions
                .Include(a => a.Coordenator)
                .Include(a => a.Course)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => CourseAction.ConvertEntityToRetrieveDto(a, a.Coordenator, a.Course))
                .ToListAsync();
            if (existingCourseActions is null || existingCourseActions.Count == 0)
            {
                _logger.LogWarning("No course actions found.");
                return Result<IEnumerable<RetrieveCourseActionDto>>
                    .Fail("Não encontrado.", "Nenhuma ação formativa encontrada.",
                    StatusCodes.Status404NotFound);
            }

            await _cache.SetAllActionsCacheAsync(existingCourseActions);

            _logger.LogInformation("Retrieved {count} course actions.", existingCourseActions.Count);
            return Result<IEnumerable<RetrieveCourseActionDto>>
                .Ok(existingCourseActions);

        }

        public async Task<Result<IEnumerable<RetrieveCourseActionDto>>> GetAllByModuleIdAsync(long moduleId)
        {
            var cachedActions = await _cache.GetCacheActionsByModuleAsync(moduleId);
            if (cachedActions is not null && cachedActions.ToList().Count != 0)
                return Result<IEnumerable<RetrieveCourseActionDto>>
                    .Ok(cachedActions);

            var existingModule = await _context.Modules.FindAsync(moduleId);
            if (existingModule is null)
            {
                _logger.LogWarning("Module with given id {id} not found.", moduleId);
                return Result<IEnumerable<RetrieveCourseActionDto>>
                    .Fail("Não encontrado.", "Módulo não encontrado.",
                        StatusCodes.Status404NotFound);
            }

            var existingCourseActions = await _context.Actions
                .Include(a => a.Coordenator)
                .Include(a => a.Course)
                    .ThenInclude(c => c.Modules)
                .Where(a => a.Course.Modules.Any(m => m.Id == moduleId))
                .ToListAsync();

            if (existingCourseActions is null || existingCourseActions.Count == 0)
            {
                _logger.LogWarning("There are no actions that incorporate the module with given id {id}", moduleId);
                return Result<IEnumerable<RetrieveCourseActionDto>>
                    .Fail("Não encontrado", "Não foram encontradas Ações de Formação que lecionem este módulo.",
                    StatusCodes.Status404NotFound);
            }

            var retrieveActions = existingCourseActions
                .AsValueEnumerable()
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => CourseAction.ConvertEntityToRetrieveDto(a, a.Coordenator, a.Course))
                .ToList();

            // update cache
            await _cache.SetActionsByModuleCacheAsync(moduleId, retrieveActions);

            return Result<IEnumerable<RetrieveCourseActionDto>>
                .Ok(retrieveActions);
        }

        public async Task<Result<RetrieveCourseActionDto>> GetByIdAsync(long id)
        {
            // Check if entry exists in cache
            var cacheAction = await _cache.GetSingleActionCacheAsync(id);
            if (cacheAction is not null)
                return Result<RetrieveCourseActionDto>
                    .Ok(cacheAction);

            // If not in cache, retrieve from database
            var existingAction = await _context.Actions
                .Include(a => a.Coordenator)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (existingAction is null)
            {
                _logger.LogWarning("Course action not found for given ID: {id}", id);
                return Result<RetrieveCourseActionDto>
                    .Fail("Não encontrado.", "Nenhuma ação formativa encontrada",
                    StatusCodes.Status404NotFound);
            }

            var retrieveAction = CourseAction.ConvertEntityToRetrieveDto(existingAction, existingAction.Coordenator, existingAction.Course);

            // Update cache
            await _cache.SetSingleActionCacheAsync(retrieveAction);

            _logger.LogInformation("Course action retrieved successfully with ID: {id}", id);
            return Result<RetrieveCourseActionDto>
                .Ok(retrieveAction);
        }

        public async Task<Result<RetrieveCourseActionDto>> UpdateAsync(UpdateCourseActionDto entityDto)
        {

            var existingCourseAction = await _context.Actions
                .Include(a => a.Course)
                .Include(a => a.Coordenator)
                .FirstOrDefaultAsync(a => a.Id == entityDto.Id);
            if (existingCourseAction is null)
            {
                _logger.LogWarning("");
                return Result<RetrieveCourseActionDto>
                .Fail("Não encontrado.", "Ação formativa não encontrada.",
                StatusCodes.Status404NotFound);
            }

            var existingCourse = await _context.Courses
                .FindAsync(entityDto.CourseId);
            if (existingCourse is null)
            {
                _logger.LogWarning("");
                return Result<RetrieveCourseActionDto>
                .Fail("Não encontrado.", "Curso não encontrado.",
                StatusCodes.Status404NotFound);
            }

            // validate unique constraint
            if (!string.IsNullOrEmpty(entityDto.Title)
                && !entityDto.Title.ToLower().Equals(existingCourseAction.Title.ToLower())
                && await _context.Actions
                .AnyAsync(a => a.Title.ToLower().Equals(entityDto.Title.ToLower())))
            {
                _logger.LogWarning("Action with title '{title}' already exists.", entityDto.Title);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Já existe uma ação com o mesmo título.");
            }

            if (!string.IsNullOrEmpty(entityDto.AdministrationCode)
                && !entityDto.AdministrationCode.Equals(existingCourseAction.AdministrationCode.ToLower())
                && await _context.Actions
                .AnyAsync(a => a.AdministrationCode == entityDto.AdministrationCode))
            {
                _logger.LogWarning("Action with administration code '{code}' already exists.", entityDto.AdministrationCode);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Já existe uma ação com o mesmo código administrativo.");
            }

            // validate enums
            if (!string.IsNullOrEmpty(entityDto.Status)
                && !EnumHelp.IsValidEnum<StatusEnum>(entityDto.Status))
            {
                _logger.LogWarning("Invalid status value: {status}", entityDto.Status);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Estado inválido.");
            }

            if (!string.IsNullOrEmpty(entityDto.Regiment)
                && !EnumHelp.IsValidEnum<RegimentTypeEnum>(entityDto.Regiment))
            {
                _logger.LogWarning("Invalid regiment value: {regiment}", entityDto.Regiment);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Regimento inválido.");
            }

            foreach (var weekDay in entityDto.WeekDays)
            {
                if (!EnumHelp.IsValidEnum<WeekDaysEnum>(weekDay))
                {
                    _logger.LogWarning("Invalid week day value: {weekDay}", weekDay);
                    return Result<RetrieveCourseActionDto>
                        .Fail("Erro de Validação", "Dia da semana inválido.");
                }
            }

            // Date formats and validation
            if (!DateOnly.TryParse(entityDto.StartDate, out var startDate))
            {
                _logger.LogWarning("Invalid start date format: {startDate}", entityDto.StartDate);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Data de início inválida.");
            }

            if (!DateOnly.TryParse(entityDto.EndDate, out var endDate))
            {
                _logger.LogWarning("Invalid end date format: {endDate}", entityDto.EndDate);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Data de fim inválida.");
            }

            // Changes made on the dates
            if (startDate.ToString("yyyy-MM-dd") != existingCourseAction.StartDate.ToString("yyyy-MM-dd") ||
                endDate.ToString("yyyy-MM-dd") != existingCourseAction.EndDate.ToString("yyyy-MM-dd"))
            {
                // Check if start date is before end date
                if (startDate >= endDate)
                {
                    _logger.LogWarning("Start date {startDate} must be before end date {endDate}.", startDate, endDate);
                    return Result<RetrieveCourseActionDto>
                        .Fail("Erro de Validação", "A data de início deve ser anterior à data de fim.");
                }

                // Check if the new start date is after today
                if (startDate < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    _logger.LogWarning("Start date {startDate} cannot be in the past.", startDate);
                    return Result<RetrieveCourseActionDto>
                        .Fail("Erro de Validação", "A data de início não pode ser anterior à data atual.");
                }
            }


            var actionEntity = CourseAction.ConvertUpdateDtoToEntity(entityDto, existingCourseAction.Coordenator, existingCourse);
            _context.Entry(existingCourseAction).CurrentValues.SetValues(actionEntity);
            await _context.SaveChangesAsync();

            var retrieveAction = CourseAction.ConvertEntityToRetrieveDto(
                actionEntity, actionEntity.Coordenator, actionEntity.Course);

            // update cache
            await _cache.RemoveActionCacheAsync(entityDto.Id);
            await _cache.SetSingleActionCacheAsync(retrieveAction);

            return Result<RetrieveCourseActionDto>
            .Ok(retrieveAction,
                "Ação de Formação Atualizada.", "Ação de formação atualizada com sucesso.");

        }

        private async Task DeleteTransactionAsync(CourseAction c)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // TODO: Remove related entities like Sessions, Enrollments, Presences etc.
                _context.Actions.Remove(c);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting course action with ID {id}.", c.Id);
                throw;
            }
            finally
            {
                await transaction.CommitAsync();
            }
        }

        public async Task<Result<IEnumerable<RetrieveCourseActionDto>>> GetAllByCourseIdAsync(long courseId)
        {
            var cachedActions = await _cache.GetCacheActionsByCourseAsync(courseId);
            if (cachedActions is not null && cachedActions.ToList().Count != 0)
                return Result<IEnumerable<RetrieveCourseActionDto>>
                    .Ok(cachedActions);

            var existingActionsWithCourse = await _context.Actions
                .Include(a => a.Course)
                .Include(a => a.Coordenator)
                .Where(a => a.Course.Id == courseId)
                .ToListAsync();
            if (existingActionsWithCourse is null
                || existingActionsWithCourse.Count == 0)
            {
                _logger.LogWarning("There are no actions that incorporate the course with given id {id}", courseId);
                return Result<IEnumerable<RetrieveCourseActionDto>>
                    .Fail("Não encontrado", "Não foram encontradas Ações de Formação neste Curso.",
                    StatusCodes.Status404NotFound);
            }

            var orderedActions = existingActionsWithCourse
                .AsValueEnumerable()
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => CourseAction.ConvertEntityToRetrieveDto(a, a.Coordenator, a.Course))
                .ToList();

            // update cache
            await _cache.SetActionsByCourseCacheAsync(courseId, orderedActions);

            return Result<IEnumerable<RetrieveCourseActionDto>>
                .Ok(orderedActions);
        }

        public async Task<Result> ChangeActionStatusAsync(long id, string status)
        {
            var existingAction = await _context.Actions.FindAsync(id);
            if (existingAction is null)
            {
                _logger.LogWarning("Action with ID {id} not found.", id);
                return Result.Fail("Não encontrado.", "Ação Formação não encontrada.",
                    StatusCodes.Status404NotFound);
            }

            if (!string.IsNullOrEmpty(status)
                && !EnumHelp.IsValidEnum<StatusEnum>(status))
            {
                _logger.LogWarning("Invalid status value: {status}", status);
                return Result.Fail("Erro de Validação", $"O estado {status} não é válido.");
            }

            // Compare status verifying if there is a change to perform
            StatusEnum s = Enum.GetValues<StatusEnum>()
                .First(e => string.Equals(e.Humanize().Transform(To.TitleCase), status,
                StringComparison.OrdinalIgnoreCase));

            if (existingAction.Status == s)
            {
                _logger.LogWarning("Action with ID {id} already has status {status}.", id, s);
                return Result.Fail("Erro de Validação", "Não alterou nenhum dado. Modifique os dados e tente novamente.");
            }

            existingAction.Status = s;
            await _context.SaveChangesAsync();

            var retrieveAction = CourseAction.ConvertEntityToRetrieveDto(existingAction, existingAction.Coordenator, existingAction.Course);

            // Update cache
            await _cache.RemoveActionCacheAsync(id);
            await _cache.SetSingleActionCacheAsync(retrieveAction);

            return Result
                .Ok("Estado da Ação Formativa alterado.", "Estado da Ação Formativa alterado com sucesso.");
        }

    }
}
