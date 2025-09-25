using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Actions.Cache;
using NERBABO.ApiService.Core.Courses.Cache;
using NERBABO.ApiService.Core.Courses.Dtos;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Modules.Cache;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using OpenTelemetry.Trace;
using ZLinq;

namespace NERBABO.ApiService.Core.Courses.Services
{
    public class CourseService(
        AppDbContext context,
        ILogger<CourseService> logger,
        ICacheCourseRepository cache,
        ICacheModuleRepository cacheModule,
        ICacheActionRepository cacheAction
        ) : ICourseService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<CourseService> _logger = logger;
        private readonly ICacheCourseRepository _cache = cache;
        private readonly ICacheModuleRepository _cacheModule = cacheModule;
        private readonly ICacheActionRepository _cacheAction = cacheAction;

        public async Task<Result<RetrieveCourseDto>> UpdateCourseModulesAsync(List<long> moduleIds, long courseId)
        {
            // course validation
            var existingCourse = await _context.Courses
                .Include(c => c.Modules)
                .Include(c => c.Frame)
                .Include(c => c.Actions)
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (existingCourse is null)
            {
                _logger.LogWarning("Course not found for the given CourseId: {CourseId}", courseId);
                return Result<RetrieveCourseDto>
                    .Fail("Não encontrado.", "Curso não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            if (!existingCourse.IsCourseActive)
            {
                _logger.LogWarning("Course is not active for the given CourseId: {CourseId}", courseId);
                return Result<RetrieveCourseDto>
                    .Fail("Erro de Validação", "Não é possível atribuir um módulo a um curso concluído ou cancelado.");
            }

            // modules validation
            List<Module> modules = [];
            float currentDuration = 0f;
            foreach (var id in moduleIds)
            {
                var m = await _context.Modules.FindAsync(id);
                if (m is null)
                {
                    _logger.LogWarning("Module not found for the given ModuleId: {id}", id);
                    return Result<RetrieveCourseDto>
                        .Fail("Não encontrado.", "Módulo não encontrado.",
                        StatusCodes.Status404NotFound);
                }

                if (!m.IsActive)
                {
                    _logger.LogWarning("Module is not active for the given ModuleId: {id}", id);
                    return Result<RetrieveCourseDto>
                        .Fail("Erro de Validação", "O módulo fornecido não está ativo.");
                }

                // Verify if the course can accommodate the module's hours
                if (!Course.CanAddModule(currentDuration, m.Hours, existingCourse.TotalDuration))
                {
                    _logger.LogWarning("Total duration exceeded for course ID: {CourseId}", courseId);
                    return Result<RetrieveCourseDto>
                        .Fail("Erro de Validação.", $"Duração total excedida. {m.Hours} excede o limite de horas do curso.");
                }
                
                if (!modules.Contains(m))
                {
                    modules.Add(m);
                    currentDuration += m.Hours;
                }

            }

            existingCourse.Modules = modules;
            await _context.SaveChangesAsync();

            var retrieveCourse = Course.ConvertEntityToRetrieveDto(existingCourse);

            // update cache
            await _cache.RemoveCourseCacheAsync(existingCourse.Id);
            await _cacheModule.RemoveModuleCacheAsync();
            await _cacheAction.RemoveActionCacheAsync();
            await _cache.SetSingleCourseCacheAsync(retrieveCourse);

            return Result<RetrieveCourseDto>
                .Ok(retrieveCourse, "Curso Atualizado.", "Módulos do Curso atualizados com sucesso.");
        }

        public async Task<Result<RetrieveCourseDto>> AssignModuleAsync(long moduleId, long courseId)
        {
            var existingModule = await _context.Modules.FindAsync(moduleId);
            if (existingModule is null)
            {
                _logger.LogWarning("Module not found for the given ModuleId: {ModuleId}", moduleId);
                return Result<RetrieveCourseDto>
                    .Fail("Não encontrado.", "Módulo não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            var existingCourse = await _context.Courses
                .Where(c => c.Id == courseId)
                .Include(c => c.Modules)
                .FirstOrDefaultAsync();
            if (existingCourse is null)
            {
                _logger.LogWarning("Course not found for the given CourseId: {CourseId}", courseId);
                return Result<RetrieveCourseDto>
                    .Fail("Não encontrado.", "Curso não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            if (!existingModule.IsActive)
            {
                _logger.LogWarning("Module is not active for the given ModuleId: {ModuleId}", moduleId);
                return Result<RetrieveCourseDto>
                    .Fail("Erro de Validação", "O módulo fornecido não está ativo.");
            }

            if (!existingCourse.IsCourseActive)
            {
                _logger.LogWarning("Course is not active for the given CourseId: {CourseId}", courseId);
                return Result<RetrieveCourseDto>
                    .Fail("Erro de Validação", "Não é possível atribuir um módulo a um curso concluído ou cancelado.");
            }

            // Verify if the module is already assigned to the course and
            if (existingCourse.IsModuleAlreadyAssigned(existingModule.Id))
            {
                _logger.LogInformation("Module already assigned to course. ModuleId: {ModuleId}, CourseId: {CourseId}", moduleId, courseId);
                return Result<RetrieveCourseDto>
                    .Fail("Erro de validação.", "O módulo já está atribuído ao curso.");
            }

            // Verify if the course can accommodate the module's hours
            if (!existingCourse.CanAddModule(existingModule.Hours))
            {
                _logger.LogWarning("Total duration exceeded for course ID: {CourseId}", courseId);
                return Result<RetrieveCourseDto>
                    .Fail("Erro de Validação.", $"Duração total excedida. Adicionar este módulo excede o limite de horas do curso.");
            }

            // Assign the module to the course
            existingCourse.Modules.Add(existingModule);
            await _context.SaveChangesAsync();

            // Update cache
            await _cache.RemoveCourseCacheAsync(existingCourse.Id);
            await _cacheModule.RemoveModuleCacheAsync();
            await _cacheAction.RemoveActionCacheAsync();

            return Result<RetrieveCourseDto>
                .Ok(Course.ConvertEntityToRetrieveDto(existingCourse),
                "Módulo Atribuído", "Módulo atribuído com sucesso ao curso.");
        }

        public async Task<Result> ChangeCourseStatusAsync(long id, string status)
        {
            var existingCourse = await _context.Courses
            .Include(c => c.Actions)
            .FirstOrDefaultAsync(c => c.Id == id);
            if (existingCourse is null)
            {
                _logger.LogWarning("Course with given id {id} not found.", id);
                return Result
                    .Fail("Não encontrado.", "Curso não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            if (!string.IsNullOrEmpty(status)
            && !EnumHelp.IsValidEnum<StatusEnum>(status))
            {
                _logger.LogWarning("Invalid Status type provided.");
                return Result
                    .Fail("Não encontrado", $"O estado {status} não é válido",
                    StatusCodes.Status404NotFound);
            }

            // Compare status verifying if there is a change to perform
            StatusEnum s = Enum.GetValues<StatusEnum>()
                .First(e => string.Equals(e.Humanize().Transform(To.TitleCase), status,
                StringComparison.OrdinalIgnoreCase));

            if (existingCourse.Status == s)
            {
                _logger.LogWarning("Tryed to perfom a update but nothing changed.");
                return Result
                    .Fail("Erro de Validação.", "Não alterou nenhum dado. Modifique os dados e tente novamente.");
            }

            // check if all actions of the course are completed before set the course as completed
            if (s.Equals(StatusEnum.Completed)
                && !existingCourse.Actions.All(a => a.Status.Equals(StatusEnum.Completed)))
            {
                _logger.LogWarning("Not all actions of the course with id {courseId} are completed.", id);
                return Result
                    .Fail("Erro de Validação.", "Nem todas as ações do curso estão concluídas. Concluía primeiro todas as ações registadas.");
            }

            // check if all actions of the course are canceled before set the course as canceled
            if (s.Equals(StatusEnum.Cancelled)
                && !existingCourse.Actions.All(a => a.Status.Equals(StatusEnum.Cancelled)))
            {
                _logger.LogWarning("Not all actions of the course with id {courseId} are canceled.", id);
                return Result
                    .Fail("Erro de Validação.", "Nem todas as ações do curso estão canceladas. Cancele primeiro todas as ações registadas.");
            }

            existingCourse.Status = s;
            await _context.SaveChangesAsync();

            // Update cache
            await _cache.RemoveCourseCacheAsync(id);
            await _cacheModule.RemoveModuleCacheAsync();
            await _cacheAction.RemoveActionCacheAsync();

            return Result
                .Ok("Curso Atualizado", "Estado do Curso atualizado com sucesso.");
        }

        public async Task<Result<RetrieveCourseDto>> CreateAsync(CreateCourseDto entityDto)
        {
            // Check title uniqueness
            if (await _context.Courses.AnyAsync(c =>
                EF.Functions.Like(c.Title, entityDto.Title)))
            {
                _logger.LogWarning("Duplicted Title detected.");
                return Result<RetrieveCourseDto>
                    .Fail("Erro de Validação.", "Já existe um curso com o mesmo título.");
            }

            // check if the frame exists
            var existingFrame = await _context.Frames.FindAsync(entityDto.FrameId);
            if (existingFrame is null)
            {
                _logger.LogWarning("Frame not found for the given FrameId.");
                return Result<RetrieveCourseDto>
                    .Fail("Enquadramento não encontrado.", "O enquadramento fornecido não existe.",
                    StatusCodes.Status404NotFound);
            }

            // check if the Habilitation Level is valid
            if (!string.IsNullOrEmpty(entityDto.MinHabilitationLevel)
                && !EnumHelp.IsValidEnum<HabilitationEnum>(entityDto.MinHabilitationLevel))
            {
                _logger.LogWarning("Invalid Habilitation Level provided.");
                return Result<RetrieveCourseDto>
                    .Fail("Erro de Validação.", "O nível mínimo do curso fornecido não é válido.");
            }

            // check if the status is valid
            if (!string.IsNullOrEmpty(entityDto.Status)
                && !EnumHelp.IsValidEnum<StatusEnum>(entityDto.Status))
            {
                _logger.LogWarning("Invalid Status provided.");
                return Result<RetrieveCourseDto>
                    .Fail("Erro de Validação.", $"O status '{entityDto.Status}' não é válido.");
            }

            // check if Destinator is valid
            foreach (var destinator in entityDto.Destinators ?? [])
            {
                if (!EnumHelp.IsValidEnum<DestinatorTypeEnum>(destinator))
                {
                    _logger.LogWarning("Invalid Destinator Type provided: {Destinator}", destinator);
                    return Result<RetrieveCourseDto>
                        .Fail("Erro de Validação.", $"O destinatário do curso, '{destinator}', fornecido não é válido.");
                }
            }

            // check and convert modules
            List<Module> modules = [];
            var currentDuration = 0f;
            foreach (var moduleId in entityDto.Modules)
            {
                var m = await _context.Modules.FindAsync(moduleId);
                if (m is null)
                {
                    _logger.LogWarning("Module with id {id} not found when creating course.", moduleId);
                    return Result<RetrieveCourseDto>
                        .Fail("Não encontrado", "Um ou mais módulos não encontrado/os.",
                        StatusCodes.Status404NotFound);
                }

                if (!m.IsActive)
                {
                    _logger.LogWarning("Module with id {1} is not active. It cannot be assigned to a Course.", moduleId);
                    return Result<RetrieveCourseDto>
                        .Fail("Erro de Validação", "Um ou mais módulos não estão ativos.");
                }

                // check if there is time to add the module to the course
                if (!Course.CanAddModule(currentDuration, m.Hours, entityDto.TotalDuration))
                {
                    _logger.LogWarning("Total duration exceeded for course total duration");
                    return Result<RetrieveCourseDto>
                        .Fail("Erro de Validação.", $"Duração total excedida. {m.Hours} excede o limite de horas do curso.");
                }

                modules.Add(m);
                currentDuration += m.Hours;
            }

            // Create course in database
            var createdCourse = await _context.Courses.AddAsync(Course.ConvertCreateDtoToEntity(entityDto, existingFrame, modules));
            await _context.SaveChangesAsync();

            var course = Course.ConvertEntityToRetrieveDto(createdCourse.Entity);

            // Update Cache
            await _cache.RemoveCourseCacheAsync(course.Id);
            await _cacheModule.RemoveModuleCacheAsync();
            await _cacheAction.RemoveActionCacheAsync();
            await _cache.SetSingleCourseCacheAsync(course);

            _logger.LogInformation("Course created successfully with ID: {CourseId}", createdCourse.Entity.Id);
            return Result<RetrieveCourseDto>
                .Ok(Course.ConvertEntityToRetrieveDto(createdCourse.Entity),
                "Curso Criado", "Curso Criado com sucesso.",
                StatusCodes.Status201Created);

        }

        public async Task<Result> DeleteAsync(long id)
        {
            // TODO: Implement related entities deletion logic
            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse is null)
            {
                _logger.LogWarning("Course not found for the given ID: {CourseId}", id);
                return Result.Fail("Curso não encontrado.", "O curso fornecido não existe.",
                    StatusCodes.Status404NotFound);
            }

            // check if there are any active actions associated with this course
            // if true dont allow delete
            if (await _context.Actions
                .Where(a => a.CourseId == id)
                .AnyAsync())
            {
                _logger.LogWarning("Tryed to delete a course that has active on going acttions, when its not possible.");
                return Result
                    .Fail("Erro de Validação", "Não pode efetuar esta ação sendo que existem ações em andamento associados a este curso.");
            }

            // dont allow to delete Completed courses
            if (existingCourse.Status == StatusEnum.Completed)
            {
                _logger.LogWarning("Tryed to delete a course that is already completed, when its not possible.");
                return Result
                    .Fail("Erro de Validação", "Não pode efetuar esta ação sendo que o curso já foi concluído.");
            }

            _context.Courses.Remove(existingCourse);
            await _context.SaveChangesAsync();

            // Update Cache
            await _cache.RemoveCourseCacheAsync(existingCourse.Id);
            await _cacheModule.RemoveModuleCacheAsync();
            await _cacheAction.RemoveActionCacheAsync();

            _logger.LogInformation("Course deleted successfully with ID: {CourseId}", id);
            return Result
                .Ok("Curso eliminado.", "Curso eliminado com sucesso.");
        }

        public async Task<Result<IEnumerable<RetrieveCourseDto>>> GetAllActiveAsync()
        {
            // try return from cache
            var cachedCourses = await _cache.GetCacheActiveCoursesAsync();
            if (cachedCourses is not null)
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Ok(cachedCourses);

            // retrieve from database
            var existingCourses = await _context.Courses
                .Include(c => c.Frame)
                .Include(c => c.Modules)
                .Include(c => c.Actions)
                .ToListAsync();

            // perform in memory logic
            var activeCourses = existingCourses
                .AsValueEnumerable()
                .Where(c => c.IsCourseActive)
                .OrderByDescending(c => c.CreatedAt)
                .Select(Course.ConvertEntityToRetrieveDto)
                .ToList();
            if (activeCourses is null || activeCourses.Count == 0)
            {
                _logger.LogWarning("There are no courses available.");
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Fail("Nenhum curso encontrado.", "Não existem cursos ativos no sistema.",
                    StatusCodes.Status404NotFound);
            }

            // update cache
            await _cache.SetActiveCoursesCacheAsync(activeCourses);

            _logger.LogInformation("Retrieved {CourseCount} active courses.", activeCourses.Count);
            return Result<IEnumerable<RetrieveCourseDto>>
                .Ok(activeCourses);
        }

        public async Task<Result<IEnumerable<RetrieveCourseDto>>> GetAllAsync()
        {
            var cachedCourses = await _cache.GetCacheAllCoursesAsync();
            if (cachedCourses is not null)
            {
                _logger.LogInformation("Retrieved active courses from cache.");
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Ok(cachedCourses);
            }

            var existingCourses = await _context.Courses
                .Include(c => c.Frame)
                .Include(c => c.Modules)
                .Include(c => c.Actions)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => Course.ConvertEntityToRetrieveDto(c))
                .ToListAsync();

            if (existingCourses is null || existingCourses.Count == 0)
            {
                _logger.LogWarning("There are no courses available.");
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Fail("Nenhum curso encontrado.", "Não existem cursos no sistema.",
                    StatusCodes.Status404NotFound);
            }

            // update cache
            await _cache.SetAllCoursesCacheAsync(existingCourses);

            _logger.LogInformation("Retrieved {CourseCount} courses.", existingCourses.Count);
            return Result<IEnumerable<RetrieveCourseDto>>
                .Ok(existingCourses);
        }

        public async Task<Result<IEnumerable<RetrieveCourseDto>>> GetAllByFrameIdAsync(long frameId)
        {
            var cachedCourses = await _cache.GetCacheCoursesByFrameAsync(frameId);
            if (cachedCourses is not null)
            {
                _logger.LogInformation("Retrieved active courses from cache.");
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Ok(cachedCourses);
            }

            var existingFrame = await _context.Frames.FindAsync(frameId);
            if (existingFrame is null)
            {
                _logger.LogWarning("Frame passed does not exist");
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Fail("Não encontrado.", "Enquadramento não foi encontrado.",
                    StatusCodes.Status404NotFound);
            }

            var existingCourses = await _context.Courses
                .Include(c => c.Frame)
                .Include(c => c.Modules)
                .Include(c => c.Actions)
                .Where(c => c.FrameId == frameId)
                .OrderBy(c => c.Status)
                    .ThenByDescending(c => c.CreatedAt)
                .Select(c => Course.ConvertEntityToRetrieveDto(c))
                .ToListAsync();

            if (existingCourses is null || existingCourses.Count == 0)
            {
                _logger.LogWarning("There are no courses available.");
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Fail("Nenhum curso encontrado.", "Não existem cursos associados a este enquadramento no sistema.",
                    StatusCodes.Status404NotFound);
            }

            // update cache
            await _cache.SetCoursesByFrameCacheAsync(frameId, existingCourses);

            _logger.LogInformation("Retrieved {CourseCount} active courses.", existingCourses.Count);
            return Result<IEnumerable<RetrieveCourseDto>>
                .Ok(existingCourses);
        }

        public async Task<Result<RetrieveCourseDto>> GetByIdAsync(long id)
        {
            // try return from cache
            var cachedCourse = await _cache.GetSingleCourseCacheAsync(id);
            if (cachedCourse is not null)
            {
                _logger.LogInformation("Retrieved course with ID {CourseId} from cache.", id);
                return Result<RetrieveCourseDto>
                    .Ok(cachedCourse);
            }

            // retrieve from database
            var existingCourse = await _context.Courses
                .Include(c => c.Frame)
                .Include(c => c.Modules)
                .Include(c => c.Actions)
                .Where(c => c.Id == id)
                .Select(c => Course.ConvertEntityToRetrieveDto(c))
                .FirstOrDefaultAsync();
            if (existingCourse is null)
            {
                _logger.LogWarning("Course not found for the given ID: {CourseId}", id);
                return Result<RetrieveCourseDto>
                    .Fail("Curso não encontrado.", "O curso fornecido não existe.",
                    StatusCodes.Status404NotFound);
            }

            // update cache
            await _cache.SetSingleCourseCacheAsync(existingCourse);
            _logger.LogInformation("Retrieved course with ID {CourseId} from database.", id);
            return Result<RetrieveCourseDto>
                .Ok(existingCourse);
        }

        public async Task<Result<IEnumerable<RetrieveCourseDto>>> GetCoursesByModuleIdAsync(long moduleId)
        {
            // try return from cache
            var cachedCourses = await _cache.GetCacheCoursesByModuleAsync(moduleId);
            if (cachedCourses is not null && cachedCourses.ToList().Count != 0)
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Ok(cachedCourses);


            var existingModule = await _context.Modules.FindAsync(moduleId);
            if (existingModule is null)
            {
                _logger.LogWarning("Module with given id not found {id}", moduleId);
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Fail("Não encontrado.", "Módulo não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            var existingCoursesWithModule = await _context.Courses
                .Include(c => c.Frame)
                .Include(c => c.Modules)
                .Include(c => c.Actions)
                .Where(c => c.Modules.Any(m => m.Id == moduleId))
                .ToListAsync();

            if (existingCoursesWithModule is null || existingCoursesWithModule.Count == 0)
            {
                return Result<IEnumerable<RetrieveCourseDto>>
                .Fail("Não encontrado.", "Não foram encontrados módulos neste curso.",
                    StatusCodes.Status404NotFound);
            }

            var courseDtos = existingCoursesWithModule
                .AsValueEnumerable()
                .OrderByDescending(c => c.IsCourseActive)
                .ThenByDescending(c => c.CreatedAt)
                .Select(Course.ConvertEntityToRetrieveDto)
                .ToList();

            // Update cache
            await _cache.SetCoursesByModuleCacheAsync(moduleId, courseDtos);

            return Result<IEnumerable<RetrieveCourseDto>>
                .Ok(courseDtos);
        }

        public async Task<Result<RetrieveCourseDto>> UnassignModuleAsync(long moduleId, long courseId)
        {
            var existingModule = await _context.Modules.FindAsync(moduleId);
            if (existingModule is null)
            {
                _logger.LogWarning("Module not found for the given ModuleId: {ModuleId}", moduleId);
                return Result<RetrieveCourseDto>
                    .Fail("Não encontrado.", "Módulo não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            var existingCourse = await _context.Courses
                .Where(c => c.Id == courseId)
                .Include(c => c.Frame)
                .Include(c => c.Modules)
                .Include(c => c.Actions)
                .FirstOrDefaultAsync();

            if (existingCourse is null)
            {
                _logger.LogWarning("Course not found for the given CourseId: {CourseId}", courseId);
                return Result<RetrieveCourseDto>
                    .Fail("Não encontrado.", "Curso não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            // Prevent module removal from active courses
            if (existingCourse.IsCourseActive)
            {
                _logger.LogWarning("Attempted to remove module from active course. CourseId: {CourseId}, ModuleId: {ModuleId}", courseId, moduleId);
                return Result<RetrieveCourseDto>
                    .Fail("Erro de Validação", "Não é possível remover um módulo de um curso que está ativo (Não Iniciado ou Em Progresso).");
            }

            // Assign the module to the course
            existingCourse.Modules.Remove(existingModule);
            await _context.SaveChangesAsync();

            // Update cache
            await _cache.RemoveCourseCacheAsync(existingCourse.Id);
            await _cacheModule.RemoveModuleCacheAsync();
            await _cacheAction.RemoveActionCacheAsync();

            return Result<RetrieveCourseDto>
                .Fail("Módulo Removido.", "Módulo removido com sucesso do curso.");
        }

        public async Task<Result<RetrieveCourseDto>> UpdateAsync(UpdateCourseDto entityDto)
        {
            // Get the existing course from the database
            var existingCourse = await _context.Courses
                .Include(c => c.Frame)
                .Include(c => c.Modules)
                .Include(c => c.Actions)
                .FirstOrDefaultAsync(c => c.Id == entityDto.Id);
            if (existingCourse is null)
            {
                _logger.LogWarning("Course not found for the given ID: {CourseId}", entityDto.Id);
                return Result<RetrieveCourseDto>
                    .Fail("Curso não encontrado.", "O curso fornecido não existe.",
                    StatusCodes.Status404NotFound);
            }

            // Check if the frame exists
            var existingFrame = await _context.Frames.FindAsync(entityDto.FrameId);
            if (existingFrame is null)
            {
                _logger.LogWarning("Frame not found for the given FrameId: {FrameId}", entityDto.FrameId);
                return Result<RetrieveCourseDto>
                    .Fail("Enquadramento não encontrado.", "O enquadramento para associar ao curso não existe.",
                    StatusCodes.Status404NotFound);
            }

            // Check if the title is unique
            if (!string.IsNullOrEmpty(entityDto.Title)
            && !existingCourse.Title.Equals(entityDto.Title, StringComparison.OrdinalIgnoreCase)
                && await _context.Courses.AnyAsync(c => EF.Functions.Like(c.Title, entityDto.Title)))
            {
                _logger.LogWarning("Duplicated Title detected for course ID: {CourseId}", entityDto.Id);
                return Result<RetrieveCourseDto>
                    .Fail("Título Duplicado.", "Já existe um curso com o mesmo título.");
            }

            // check if the Habilitation Level is valid
            if (!string.IsNullOrEmpty(entityDto.MinHabilitationLevel)
                && !EnumHelp.IsValidEnum<HabilitationEnum>(entityDto.MinHabilitationLevel))
            {
                _logger.LogWarning("Invalid Habilitation Level provided.");
                return Result<RetrieveCourseDto>
                    .Fail("Nível inválido.", $"O nível mínimo do curso '{entityDto.MinHabilitationLevel}' não é válido.");
            }

            // check if the status is valid
            if (!string.IsNullOrEmpty(entityDto.Status)
                && !EnumHelp.IsValidEnum<StatusEnum>(entityDto.Status))
            {
                _logger.LogWarning("Invalid Status provided.");
                return Result<RetrieveCourseDto>
                    .Fail("Status inválido.", $"O status '{entityDto.Status}' não é válido.");
            }

            // check if Destinator is valid
            List<DestinatorTypeEnum> newDestinators = [];
            foreach (var destinator in entityDto.Destinators ?? [])
            {
                if (!EnumHelp.IsValidEnum<DestinatorTypeEnum>(destinator))
                {
                    _logger.LogWarning("Invalid Destinator Type provided: {Destinator}", destinator);
                    return Result<RetrieveCourseDto>
                        .Fail("Destinatário inválido.", $"O destinatário do curso '{destinator}' não é válido.");
                }
                newDestinators.Add(destinator.DehumanizeTo<DestinatorTypeEnum>());
            }

            // check and convert modules
            List<Module> modules = [];
            var currentDuration = 0f;
            foreach (var moduleId in entityDto.Modules)
            {
                var m = await _context.Modules.FindAsync(moduleId);
                if (m is null)
                {
                    _logger.LogWarning("Module with id {id} not found when updating course.", moduleId);
                    return Result<RetrieveCourseDto>
                        .Fail("Não encontrado", "Um ou mais módulos não encontrado/os.",
                        StatusCodes.Status404NotFound);
                }

                if (!m.IsActive)
                {
                    _logger.LogWarning("Module with id {1} is not active. It cannot be assigned to a Course.", moduleId);
                    return Result<RetrieveCourseDto>
                        .Fail("Erro de Validação", "Um ou mais módulos não estão ativos.");
                }

                // check if there is time to add the module to the course
                if (!Course.CanAddModule(currentDuration, m.Hours, entityDto.TotalDuration))
                {
                    _logger.LogWarning("Total duration exceeded for course total duration");
                    return Result<RetrieveCourseDto>
                        .Fail("Erro de Validação.", $"Duração total excedida. {m.Hours} excede o limite de horas do curso.");
                }

                modules.Add(m);
                currentDuration += m.Hours;
            }

            // Selective field updates - only update fields that have changed
            bool hasChanges = false;

            // Update FrameId if changed
            if (existingCourse.FrameId != entityDto.FrameId)
            {
                existingCourse.FrameId = entityDto.FrameId;
                existingCourse.Frame = existingFrame;
                hasChanges = true;
            }

            // Update Title if changed
            if (!string.Equals(existingCourse.Title, entityDto.Title))
            {
                existingCourse.Title = entityDto.Title;
                hasChanges = true;
            }

            // Update Objectives if changed
            if (!string.Equals(existingCourse.Objectives, entityDto.Objectives))
            {
                existingCourse.Objectives = entityDto.Objectives ?? "";
                hasChanges = true;
            }

            // Update TotalDuration if changed
            if (Math.Abs(existingCourse.TotalDuration - entityDto.TotalDuration) > 0.01f)
            {
                existingCourse.TotalDuration = entityDto.TotalDuration;
                hasChanges = true;
            }

            // Update Status if changed
            var newStatus = entityDto.Status.DehumanizeTo<StatusEnum>();
            if (existingCourse.Status != newStatus)
            {
                // check if all actions of the course are completed before set the course as completed
                if (newStatus.Equals(StatusEnum.Completed)
                    && !existingCourse.Actions.All(a => a.Status.Equals(StatusEnum.Completed)))
                {
                    _logger.LogWarning("Not all actions of the course with id {courseId} are completed.", entityDto.Id);
                    return Result<RetrieveCourseDto>
                        .Fail("Erro de Validação.", "Nem todas as ações do curso estão concluídas. Concluía primeiro todas as ações registadas.");
                }

                // check if all actions of the course are canceled before set the course as canceled
                if (newStatus.Equals(StatusEnum.Cancelled)
                    && !existingCourse.Actions.All(a => a.Status.Equals(StatusEnum.Cancelled)))
                {
                    _logger.LogWarning("Not all actions of the course with id {courseId} are canceled.", entityDto.Id);
                    return Result<RetrieveCourseDto>
                        .Fail("Erro de Validação.", "Nem todas as ações do curso estão canceladas. Cancele primeiro todas as ações registadas.");
                }
                existingCourse.Status = newStatus;
                hasChanges = true;
            }

            // Update Area if changed
            if (!string.Equals(existingCourse.Area, entityDto.Area))
            {
                existingCourse.Area = entityDto.Area ?? string.Empty;
                hasChanges = true;
            }

            // Update MinHabilitationLevel if changed
            var newHabilitationLevel = entityDto.MinHabilitationLevel.DehumanizeTo<HabilitationEnum>();
            if (existingCourse.MinHabilitationLevel != newHabilitationLevel)
            {
                existingCourse.MinHabilitationLevel = newHabilitationLevel;
                hasChanges = true;
            }

            // Update Destinators if changed
            if (!existingCourse.Destinators.SequenceEqual(newDestinators))
            {
                existingCourse.Destinators = newDestinators;
                hasChanges = true;
            }

            // Update Modules if changed - but prevent changes if course is active
            var existingModuleIds = existingCourse.Modules.Select(m => m.Id).OrderBy(id => id).ToList();
            var newModuleIds = entityDto.Modules.OrderBy(id => id).ToList();
            if (!existingModuleIds.SequenceEqual(newModuleIds))
            {
                // Check if course is active and modules are being removed
                var removedModuleIds = existingModuleIds.Except(newModuleIds).ToList();
                if (existingCourse.IsCourseActive && removedModuleIds.Any())
                {
                    _logger.LogWarning("Attempted to remove modules from active course. CourseId: {CourseId}", entityDto.Id);
                    return Result<RetrieveCourseDto>
                        .Fail("Erro de Validação", "Não é possível remover módulos de um curso que está ativo (Não Iniciado ou Em Progresso).");
                }
                
                existingCourse.Modules = modules;
                hasChanges = true;
            }

            // Return fail result if no changes were detected
            if (!hasChanges)
            {
                _logger.LogInformation("No changes detected for Course with ID {id}. No update performed.", entityDto.Id);
                return Result<RetrieveCourseDto>
                    .Fail("Nenhuma alteração detetada.", "Não foi alterado nenhum dado. Modifique os dados e tente novamente.",
                    StatusCodes.Status400BadRequest);
            }

            // Update UpdatedAt and save changes
            existingCourse.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var updatedCourse = Course.ConvertEntityToRetrieveDto(existingCourse);

            // update cache
            await _cache.RemoveCourseCacheAsync(existingCourse.Id);
            await _cacheModule.RemoveModuleCacheAsync();
            await _cacheAction.RemoveActionCacheAsync();
            await _cache.SetSingleCourseCacheAsync(updatedCourse);

            _logger.LogInformation("Course updated successfully with ID: {CourseId}", existingCourse.Id);
            return Result<RetrieveCourseDto>
                .Ok(updatedCourse, "Curso Atualizado", "Curso atualizado com sucesso.");
        }

        public async Task<Result<KpisCourseDto>> GetKpisAsync(long courseId)
        {
            var existingCourse = await _context.Courses
                .Include(c => c.Actions)
                    .ThenInclude(a => a.ActionEnrollments)
                        .ThenInclude(ae => ae.Participations)
                .Include(c => c.Actions)
                    .ThenInclude(a => a.ModuleTeachings)
                        .ThenInclude(mt => mt.Sessions)
                .FirstOrDefaultAsync(c => c.Id == courseId);
            
            if (existingCourse is null)
            {
                _logger.LogWarning("Course with ID {id} not found.", courseId);
                return Result<KpisCourseDto>
                    .Fail("Não encontrado.", "Curso não encontrado.",
                    StatusCodes.Status404NotFound);
            }

            // Calculate aggregated KPIs from all actions of this course
            var kpis = new KpisCourseDto
            {
                TotalStudents = existingCourse.Actions.Sum(a => a.TotalStudents),
                TotalApproved = existingCourse.Actions.Sum(a => a.TotalApproved),
                TotalVolumeHours = existingCourse.Actions.Sum(a => a.TotalVolumeHours),
                TotalVolumeDays = existingCourse.Actions.Sum(a => a.TotalVolumeDays)
            };

            return Result<KpisCourseDto>
                .Ok(kpis);
        }
    }
}
