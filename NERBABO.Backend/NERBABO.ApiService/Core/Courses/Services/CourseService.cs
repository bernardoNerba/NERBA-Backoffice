using Humanizer;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Courses.Dtos;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Core.Frames.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;
using OpenTelemetry.Trace;
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

        public async Task<Result<RetrieveCourseDto>> CreateAsync(CreateCourseDto entityDto)
        {
            // Check title uniqueness
            if (await _context.Courses.AnyAsync(c => 
                EF.Functions.Like(c.Title, entityDto.Title)))
            {
                _logger.LogWarning("Duplicted Title detected.");
                return Result<RetrieveCourseDto>
                    .Fail("Título Duplicado.","Já existe um curso com o mesmo título.");
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
                    .Fail("Nível inválido.", "O nível mínimo do curso fornecido não é válido.");
            }

            // Create course in database
            var createdCourse = await _context.Courses.AddAsync(Course.ConvertCreateDtoToEntity(entityDto, existingFrame));
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
                .Where(c => c.Status)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => Course.ConvertEntityToRetrieveDto(c))
                .ToListAsync();

            if (existingCourses is null || existingCourses.Count == 0)
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
                .Ok(existingCourses);
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

        public async Task<Result<RetrieveCourseDto>> UpdateAsync(UpdateCourseDto entityDto)
        {
            // Get the existing course from the database
            var existingCourse = await _context.Courses
                .Include(c => c.Frame)
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
                    .Fail("Nível inválido.", "O nível mínimo do curso fornecido não é válido.");
            }

            _context.Entry(existingCourse).CurrentValues.SetValues(Course.ConvertUpdateDtoToEntity(entityDto, existingFrame));
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
        }
    }
}
