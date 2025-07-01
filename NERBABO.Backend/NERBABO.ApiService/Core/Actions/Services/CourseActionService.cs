using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Actions.Dtos;
using NERBABO.ApiService.Core.Actions.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Actions.Services
{
    public class CourseActionService(
        AppDbContext context,
        ILogger<CourseActionService> logger
        ) : ICourseActionService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<CourseActionService> _logger = logger;

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
                && await _context.Actions
                .AnyAsync(a => a.Title.ToLower().Equals(entityDto.Title.ToLower())))
            {
                _logger.LogWarning("Action with title '{title}' already exists.", entityDto.Title);
                return Result<RetrieveCourseActionDto>
                    .Fail("Erro de Validação", "Já existe uma ação com o mesmo título.");
            }

            if (!string.IsNullOrEmpty(entityDto.AdministrationCode)
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
            _context.Actions.Add(action);
            _context.SaveChanges();

            _logger.LogInformation("Course action created successfully with ID: {id}", action.Id);
            return Result<RetrieveCourseActionDto>
                .Ok(CourseAction.ConvertEntityToRetrieveDto(action, existingCoordenator, existingCourse));
        }

        public Task<Result> DeleteAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IEnumerable<RetrieveCourseActionDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<RetrieveCourseActionDto>> GetByIdAsync(long id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<RetrieveCourseActionDto>> UpdateAsync(UpdateCourseActionDto entityDto)
        {
            throw new NotImplementedException();
        }
    }
}
