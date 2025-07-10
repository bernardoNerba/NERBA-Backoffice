using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Courses.Dtos;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Modules.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;
using ZLinq;

namespace NERBABO.ApiService.Core.Courses.Services
{
    public class CourseService(
        AppDbContext context,
        ILogger<CourseService> logger,
        ICacheService cache
        ) : ICourseService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<CourseService> _logger = logger;
        private readonly ICacheService _cache = cache;

        public async Task<Result<RetrieveCourseDto>> UpdateCourseModulesAsync(List<long> moduleIds, long courseId)
        {
            // course validation
            var existingCourse = await _context.Courses
                .Include(c => c.Modules)
                .Include(c => c.Frame)
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
                        .Fail("Erro de Validação.", $"Duração total excedida. Tentou adicionar {m.Hours} quando {existingCourse.CurrentDuration}/{existingCourse.TotalDuration}");
                }
                if (!modules.Contains(m))
                    modules.Add(m);
            }

            existingCourse.Modules = modules;
            await _context.SaveChangesAsync();

            var retrieveCourse = Course.ConvertEntityToRetrieveDto(existingCourse);

            // update cache
            await DeleteCacheAsync(existingCourse.Id);
            await _cache.SetAsync($"course:{existingCourse.Id}", retrieveCourse, TimeSpan.FromMinutes(30));

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
                    .Fail("Erro de Validação.", $"Duração total excedida. Tentou adicionar {existingModule.Hours} quando {existingCourse.CurrentDuration}/{existingCourse.TotalDuration}");
            }

            // Assign the module to the course
            existingCourse.Modules.Add(existingModule);
            await _context.SaveChangesAsync();

            // Update cache
            await DeleteCacheAsync(existingCourse.Id);


            return Result<RetrieveCourseDto>
                .Ok(Course.ConvertEntityToRetrieveDto(existingCourse),
                "Módulo Atribuído", "Módulo atribuído com sucesso ao curso.");
        }

        public async Task<Result> ChangeCourseStatusAsync(long id, string status)
        {
            var existingCourse = await _context.Courses.FindAsync(id);
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

            existingCourse.Status = s;
            await _context.SaveChangesAsync();


            // Update cache
            await _cache.SetAsync($"course:{existingCourse.Id}", existingCourse, TimeSpan.FromMinutes(30));
            await DeleteCacheAsync();

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
                        .Fail("Erro de Validação.", $"Duração total excedida. Tentou adicionar {m.Hours}h quando {currentDuration}h/{entityDto.TotalDuration}h");
                }

                modules.Add(m);
                currentDuration += m.Hours;
            }

            // Create course in database
            var createdCourse = await _context.Courses.AddAsync(Course.ConvertCreateDtoToEntity(entityDto, existingFrame, modules));
            await _context.SaveChangesAsync();

            var course = Course.ConvertEntityToRetrieveDto(createdCourse.Entity);

            // Update Cache
            await _cache.SetAsync($"course:{course.Id}", course, TimeSpan.FromMinutes(30));
            await DeleteCacheAsync();

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
            await DeleteCacheAsync(id);

            _logger.LogInformation("Course deleted successfully with ID: {CourseId}", id);
            return Result
                .Ok("Curso eliminado.", "Curso eliminado com sucesso.");
        }

        public async Task<Result<IEnumerable<RetrieveCourseDto>>> GetAllActiveAsync()
        {
            var cacheKey = "course:list:active";
            var cachedCourses = await _cache.GetAsync<IEnumerable<RetrieveCourseDto>>(cacheKey);
            if (cachedCourses is not null)
            {
                _logger.LogInformation("Retrieved active courses from cache.");
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Ok(cachedCourses);
            }

            var existingCourses = await _context.Courses
                .Include(c => c.Frame)
                .Include(c => c.Modules)
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
            await _cache.SetAsync(cacheKey, existingCourses, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Retrieved {CourseCount} active courses.", existingCourses.Count);
            return Result<IEnumerable<RetrieveCourseDto>>
                .Ok(activeCourses);
        }

        public async Task<Result<IEnumerable<RetrieveCourseDto>>> GetAllAsync()
        {
            var cacheKey = "course:list";
            var cachedCourses = await _cache.GetAsync<IEnumerable<RetrieveCourseDto>>(cacheKey);
            if (cachedCourses is not null)
            {
                _logger.LogInformation("Retrieved active courses from cache.");
                return Result<IEnumerable<RetrieveCourseDto>>
                    .Ok(cachedCourses);
            }

            var existingCourses = await _context.Courses
                .Include(c => c.Frame)
                .Include(c => c.Modules)
                .OrderBy(c => c.Status)
                    .ThenByDescending(c => c.CreatedAt)
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
            await _cache.SetAsync(cacheKey, existingCourses, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Retrieved {CourseCount} courses.", existingCourses.Count);
            return Result<IEnumerable<RetrieveCourseDto>>
                .Ok(existingCourses);
        }

        public async Task<Result<IEnumerable<RetrieveCourseDto>>> GetAllByFrameIdAsync(long frameId)
        {
            var cacheKey = $"course:list:frame:{frameId}";
            var cachedCourses = await _cache.GetAsync<IEnumerable<RetrieveCourseDto>>(cacheKey);
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
            await _cache.SetAsync(cacheKey, existingCourses, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Retrieved {CourseCount} active courses.", existingCourses.Count);
            return Result<IEnumerable<RetrieveCourseDto>>
                .Ok(existingCourses);
        }

        public async Task<Result<RetrieveCourseDto>> GetByIdAsync(long id)
        {
            // try return from cache
            var cacheKey = $"course:{id}";
            var cachedCourse = await _cache.GetAsync<RetrieveCourseDto>(cacheKey);
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
            await _cache.SetAsync(cacheKey, existingCourse, TimeSpan.FromMinutes(30));
            _logger.LogInformation("Retrieved course with ID {CourseId} from database.", id);
            return Result<RetrieveCourseDto>
                .Ok(existingCourse);

        }

        public async Task<Result<IEnumerable<RetrieveCourseDto>>> GetCoursesByModuleIdAsync(long moduleId)
        {
            // try return from cache
            var cacheKey = $"courses:list:module:{moduleId}";
            var cachedCourses = await _cache.GetAsync<List<RetrieveCourseDto>>(cacheKey);
            if (cachedCourses is not null && cachedCourses.Count != 0)
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
                .Include(c => c.Modules)
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
            await _cache.SetAsync(cacheKey, courseDtos, TimeSpan.FromMinutes(30));

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
                .Include(c => c.Modules)
                .FirstOrDefaultAsync();

            if (existingCourse is null)
            {
                _logger.LogWarning("Course not found for the given CourseId: {CourseId}", courseId);
                return Result<RetrieveCourseDto>
                    .Fail("Não encontrado.", "Curso não encontrado.",
                    StatusCodes.Status404NotFound);
            }


            if (!existingCourse.IsCourseActive)
            {
                _logger.LogWarning("Module is not active for the given ModuleId: {ModuleId}", moduleId);
                return Result<RetrieveCourseDto>
                    .Fail("Erro de Validação", "Não é possível remover um módulo a um curso concluído ou cancelado.");
            }

            // Assign the module to the course
            existingCourse.Modules.Remove(existingModule);
            await _context.SaveChangesAsync();

            // Update cache
            await DeleteCacheAsync(existingCourse.Id);

            return Result<RetrieveCourseDto>
                .Fail("Módulo Removido.", "Módulo removido com sucesso do curso.");
        }

        public async Task<Result<RetrieveCourseDto>> UpdateAsync(UpdateCourseDto entityDto)
        {
            // Get the existing course from the database
            var existingCourse = await _context.Courses
                .Include(c => c.Frame)
                .Include(c => c.Modules)
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

            // Check if the title is being changed and if it is unique
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
            foreach (var destinator in entityDto.Destinators ?? [])
            {
                if (!EnumHelp.IsValidEnum<DestinatorTypeEnum>(destinator))
                {
                    _logger.LogWarning("Invalid Destinator Type provided: {Destinator}", destinator);
                    return Result<RetrieveCourseDto>
                        .Fail("Destinatário inválido.", $"O destinatário do curso '{destinator}' não é válido.");
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
                        .Fail("Erro de Validação.", $"Duração total excedida. Tentou adicionar {m.Hours}h quando {currentDuration}h/{entityDto.TotalDuration}h");
                }

                modules.Add(m);
            }

            var newValues = Course.ConvertUpdateDtoToEntity(entityDto, existingFrame);
            _context.Entry(existingCourse).CurrentValues.SetValues(newValues);
            existingCourse.Modules = modules;
            await _context.SaveChangesAsync();

            var updatedCourse = Course.ConvertEntityToRetrieveDto(existingCourse);

            // update cache
            await _cache.SetAsync($"course:{existingCourse.Id}", updatedCourse, TimeSpan.FromMinutes(30));
            await DeleteCacheAsync();

            _logger.LogInformation("Course updated successfully with ID: {CourseId}", existingCourse.Id);
            return Result<RetrieveCourseDto>
                .Ok(updatedCourse, "Curso Atualizado", "Curso atualizado com sucesso.");
        }

        private async Task DeleteCacheAsync(long? id = null)
        {
            if (id is not null)
                await _cache.RemoveAsync($"course:{id}");

            await _cache.RemoveAsync("course:list");
            await _cache.RemoveAsync("course:list:active");
            await _cache.RemovePatternAsync("course:list:frame:*");
            await _cache.RemovePatternAsync("courses:list:module:*");
        }
    }
}
