using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Courses.Dtos;
using NERBABO.ApiService.Core.Courses.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

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
            if (await _context.Courses.AnyAsync(c => c.Title.Equals(entityDto.Title
                , StringComparison.OrdinalIgnoreCase)))
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
            await _cache.RemoveAsync("course:list");

            _logger.LogInformation("Course created successfully with ID: {CourseId}", createdCourse.Entity.Id);
            return Result<RetrieveCourseDto>
                .Ok(Course.ConvertEntityToRetrieveDto(createdCourse.Entity));

        }

        public Task<Result> DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<RetrieveCourseDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<RetrieveCourseDto>> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<RetrieveCourseDto>> UpdateAsync(UpdateCourseDto entityDto)
        {
            throw new NotImplementedException();
        }
    }
}
