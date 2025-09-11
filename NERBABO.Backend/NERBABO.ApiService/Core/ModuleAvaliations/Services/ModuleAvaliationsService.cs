using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.ModuleAvaliations.Dtos;
using NERBABO.ApiService.Core.ModuleAvaliations.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.ModuleAvaliations.Services;

public class ModuleAvaliationsService(
    ILogger<ModuleAvaliationsService> logger,
    AppDbContext context
) : IModuleAvaliationsService
{
    private readonly ILogger<ModuleAvaliationsService> _logger = logger;
    private readonly AppDbContext _context = context;

    public async Task<Result<IEnumerable<AvaliationsByModuleDto>>> GetByActionIdAsync(long actionId)
    {
        var existingAvaliations = await _context.ModuleAvaliations
            .AsNoTracking()
            .Include(ma => ma.ActionEnrollment).ThenInclude(ae => ae.Student).ThenInclude(s => s.Person)
            .Include(ma => ma.ModuleTeaching).ThenInclude(mt => mt.Module)
            .Include(ma => ma.ModuleTeaching).ThenInclude(mt => mt.Teacher).ThenInclude(t => t.Person)
            .Where(ma => ma.ActionEnrollment.ActionId == actionId)
            .OrderBy(ma => ma.Id)
            .ToListAsync();

        var groupedAvaliations = existingAvaliations
            .AsValueEnumerable()
            .GroupBy(ma => new { 
                ma.Id,
                ma.ActionEnrollment.ActionId, 
                ma.ModuleTeaching.ModuleId,
                ma.ModuleTeaching.Module.Name,
                ma.ModuleTeaching.Teacher.PersonId,
                ma.ModuleTeaching.Teacher.Person.FullName
            })
            .Select(g => new AvaliationsByModuleDto
            {
                ActionId = g.Key.ActionId,
                ModuleId = g.Key.ModuleId,
                ModuleName = g.Key.Name,
                TeacherPersonId = g.Key.PersonId,
                TeacherName = g.Key.FullName,
                Gradings = [.. g.Select(ma => new GradingInfoDto
                {
                    Id = ma.Id,
                    StudentPersonId = ma.ActionEnrollment.Student.PersonId,
                    StudentName = ma.ActionEnrollment.Student.Person.FullName,
                    Grade = ma.Grade,
                    Evaluated = ma.Evaluated,
                    ModuleAvaliationId = ma.Id
                })]
            })
            .ToList();

        _logger.LogInformation("Fetched avaliations by action with id: {actionId}. There are {avaliationsCount} modules.", actionId, groupedAvaliations.Count);

        return Result<IEnumerable<AvaliationsByModuleDto>>
            .Ok(groupedAvaliations);
    }

    public async Task<Result<RetrieveModuleAvaliationDto>> UpdateAsync(UpdateModuleAvaliationDto dto)
    {
        var existingAvaliation = await _context.ModuleAvaliations
            .Include(ma => ma.ActionEnrollment).ThenInclude(ae => ae.Student).ThenInclude(s => s.Person)
            .Include(ma => ma.ModuleTeaching).ThenInclude(mt => mt.Module)
            .Include(ma => ma.ModuleTeaching).ThenInclude(mt => mt.Teacher).ThenInclude(t => t.Person)
            .FirstOrDefaultAsync(ma => ma.Id == dto.Id);

        if (existingAvaliation == null)
        {
            _logger.LogWarning("Module avaliation with id {id} not found", dto.Id);
            return Result<RetrieveModuleAvaliationDto>.Fail("Não encontrado.", 
                "Avaliação de módulo não encontrada", StatusCodes.Status404NotFound);
        }

        existingAvaliation.Grade = dto.Grade;
        existingAvaliation.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully updated module avaliation with id: {id}", dto.Id);

            return Result<RetrieveModuleAvaliationDto>
                .Ok(ModuleAvaliation.ConvertEntityToEntityDto(existingAvaliation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating module avaliation with id: {id}", dto.Id);
            return Result<RetrieveModuleAvaliationDto>.Fail("Erro interno", 
                "Erro ao atualizar avaliação de módulo", StatusCodes.Status500InternalServerError);
        }
    }
}
