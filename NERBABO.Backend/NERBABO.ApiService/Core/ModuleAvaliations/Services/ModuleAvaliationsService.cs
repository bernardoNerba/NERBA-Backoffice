using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.ModuleAvaliations.Dtos;
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
            .ToListAsync();

        var groupedAvaliations = existingAvaliations
            .AsValueEnumerable()
            .GroupBy(ma => new { 
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
                    StudentPersonId = ma.ActionEnrollment.Student.PersonId,
                    StudentName = ma.ActionEnrollment.Student.Person.FullName,
                    Grade = ma.Grade,
                    Evaluated = ma.Evaluated
                })]
            })
            .ToList();

        _logger.LogInformation("Fetched avaliations by action with id: {actionId}. There are {avaliationsCount} modules.", actionId, groupedAvaliations.Count);

        return Result<IEnumerable<AvaliationsByModuleDto>>
            .Ok(groupedAvaliations);
    }
}
