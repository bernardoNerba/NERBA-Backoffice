using System;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.ModuleTeachings.Dtos;
using NERBABO.ApiService.Core.ModuleTeachings.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.ModuleTeachings.Services;

public class ModuleTeachingService(
    AppDbContext context,
    ILogger<ModuleTeachingService> logger
) : IModuleTeachingService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<ModuleTeachingService> _logger = logger;

    public async Task<Result<RetrieveModuleTeachingDto>> CreateAsync(CreateModuleTeachingDto entityDto)
    {
        // Check if Module exists
        var existingModule = await _context.Modules
            .FirstOrDefaultAsync(m => m.Id == entityDto.ModuleId);
        if (existingModule is null)
        {
            _logger.LogWarning("Module with ID {ModuleId} not found during ModuleTeaching creation.", entityDto.ModuleId);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Não encontrado.", "Módulo não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // Check if Action exists
        var existingAction = await _context.Actions
            .Include(a => a.Course)
                .ThenInclude(a => a.Modules)
            .FirstOrDefaultAsync(a => a.Id == entityDto.ActionId);
        if (existingAction is null)
        {
            _logger.LogWarning("Action with ID {ActionId} not found during ModuleTeaching creation.", entityDto.ActionId);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Não encontrado.", "Ação Formação não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // Check if Teacher exists
        var existingTeacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Id == entityDto.TeacherId);
        if (existingTeacher is null)
        {
            _logger.LogWarning("Teacher with ID {TeacherId} not found during ModuleTeaching creation.", entityDto.TeacherId);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Não encontrado.", "Formador não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // Check if the Action has the Module
        if (!existingAction.Course.Modules.Any(m => m.Id == existingModule.Id))
        {
            _logger.LogWarning("Module with ID {ModuleId} is not associated with Action {ActionId} course.", entityDto.ModuleId, entityDto.ActionId);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Erro de Validação.",
                "Nesta Ação não é lecionado o Módulo Fornecido. Adicione o Módulo ao Curso e tente novamente.");
        }

        // Check if the Action Module already has a Teacher associated with it
        var alreadyTeacherAssociated = await _context.ModuleTeachings.AnyAsync(mt =>
            mt.ActionId == existingAction.Id
            && mt.ModuleId == existingModule.Id);
        if (alreadyTeacherAssociated)
        {
            _logger.LogWarning("Teacher already associated with Module {ModuleId} in Action {ActionId}.", entityDto.ModuleId, entityDto.ActionId);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Erro de Validação.",
                "Já existe um Formador associado a este Módulo nesta Ação.");
        }

        var createdEntity = _context.ModuleTeachings.Add(ModuleTeaching.ConvertCreateDtoToEntity(entityDto, existingTeacher, existingModule, existingAction));
        await _context.SaveChangesAsync();

        var retrieveModuleTeaching = ModuleTeaching.ConvertEntityToRetrieveDto(createdEntity.Entity);

        _logger.LogInformation("ModuleTeaching created successfully. Teacher {TeacherId} associated with Module {ModuleId} in Action {ActionId}.", entityDto.TeacherId, entityDto.ModuleId, entityDto.ActionId);

        return Result<RetrieveModuleTeachingDto>
            .Ok(retrieveModuleTeaching, "Formador associado ao Módulo com sucesso.",
            $"O Formador foi associado ao Módulo da Ação Formação {existingAction.ActionNumber} - {existingAction.Locality} com sucesso.");
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var existingModuleTeaching = await _context.ModuleTeachings
            .Include(mt => mt.Teacher)
                .ThenInclude(t => t.Person)
            .Include(mt => mt.Module)
            .Include(mt => mt.Action)
            .FirstOrDefaultAsync(mt => mt.Id == id);

        if (existingModuleTeaching is null)
        {
            _logger.LogWarning("ModuleTeaching with ID {Id} not found for deletion.", id);
            return Result
                .Fail("Não encontrado.", "Associação Formador-Módulo não encontrada.",
                StatusCodes.Status404NotFound);
        }

        _context.ModuleTeachings.Remove(existingModuleTeaching);
        await _context.SaveChangesAsync();

        _logger.LogInformation("ModuleTeaching with ID {Id} deleted successfully.", id);

        return Result
            .Ok("Associação eliminada.", "A associação entre o Formador e o Módulo foi eliminada com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveModuleTeachingDto>>> GetAllAsync()
    {
        var existingModuleTeachings = await _context.ModuleTeachings
            .Include(mt => mt.Teacher)
                .ThenInclude(t => t.Person)
            .Include(mt => mt.Module)
            .Include(mt => mt.Action)
                .ThenInclude(a => a.Course)
            .OrderByDescending(mt => mt.CreatedAt)
            .Select(mt => ModuleTeaching.ConvertEntityToRetrieveDto(mt))
            .ToListAsync();

        if (!existingModuleTeachings.Any())
        {
            _logger.LogInformation("No ModuleTeachings found in the system.");
            return Result<IEnumerable<RetrieveModuleTeachingDto>>
                .Fail("Não encontrado.", "Não foram encontradas associações Formador-Módulo no sistema.",
                StatusCodes.Status404NotFound);
        }

        return Result<IEnumerable<RetrieveModuleTeachingDto>>
            .Ok(existingModuleTeachings);
    }

    public async Task<Result<RetrieveModuleTeachingDto>> GetByIdAsync(long id)
    {
        var existingModuleTeaching = await _context.ModuleTeachings
            .Include(mt => mt.Teacher)
                .ThenInclude(t => t.Person)
            .Include(mt => mt.Module)
            .Include(mt => mt.Action)
                .ThenInclude(a => a.Course)
            .FirstOrDefaultAsync(mt => mt.Id == id);

        if (existingModuleTeaching is null)
        {
            _logger.LogWarning("ModuleTeaching with ID {Id} not found.", id);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Não encontrado.", "Associação Formador-Módulo não encontrada.",
                StatusCodes.Status404NotFound);
        }

        var retrieveModuleTeaching = ModuleTeaching.ConvertEntityToRetrieveDto(existingModuleTeaching);

        return Result<RetrieveModuleTeachingDto>
            .Ok(retrieveModuleTeaching);
    }

    public async Task<Result<RetrieveModuleTeachingDto>> UpdateAsync(UpdateModuleTeachingDto entityDto)
    {
        var existingModuleTeaching = await _context.ModuleTeachings
            .Include(mt => mt.Teacher)
                .ThenInclude(t => t.Person)
            .Include(mt => mt.Module)
            .Include(mt => mt.Action)
                .ThenInclude(a => a.Course)
                    .ThenInclude(c => c.Modules)
            .FirstOrDefaultAsync(mt => mt.Id == entityDto.Id);

        if (existingModuleTeaching is null)
        {
            _logger.LogWarning("ModuleTeaching with ID {Id} not found for update.", entityDto.Id);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Não encontrado.", "Associação Formador-Módulo não encontrada.",
                StatusCodes.Status404NotFound);
        }

        // Check if Module exists
        var existingModule = await _context.Modules
            .FirstOrDefaultAsync(m => m.Id == entityDto.ModuleId);
        if (existingModule is null)
        {
            _logger.LogWarning("Module with ID {ModuleId} not found during ModuleTeaching update.", entityDto.ModuleId);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Não encontrado.", "Módulo não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // Check if Action exists
        var existingAction = await _context.Actions
            .Include(a => a.Course)
                .ThenInclude(a => a.Modules)
            .FirstOrDefaultAsync(a => a.Id == entityDto.ActionId);
        if (existingAction is null)
        {
            _logger.LogWarning("Action with ID {ActionId} not found during ModuleTeaching update.", entityDto.ActionId);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Não encontrado.", "Ação Formação não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // Check if Teacher exists
        var existingTeacher = await _context.Teachers
            .Include(t => t.Person)
            .FirstOrDefaultAsync(t => t.Id == entityDto.TeacherId);
        if (existingTeacher is null)
        {
            _logger.LogWarning("Teacher with ID {TeacherId} not found during ModuleTeaching update.", entityDto.TeacherId);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Não encontrado.", "Formador não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // Check if the Action has the Module
        if (!existingAction.Course.Modules.Any(m => m.Id == existingModule.Id))
        {
            _logger.LogWarning("Module with ID {ModuleId} is not associated with Action {ActionId} course during update.", entityDto.ModuleId, entityDto.ActionId);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Erro de Validação.",
                "Nesta Ação não é lecionado o Módulo Fornecido. Adicione o Módulo ao Curso e tente novamente.");
        }

        // Check if another ModuleTeaching already exists for this Action-Module combination (excluding current one)
        var alreadyTeacherAssociated = await _context.ModuleTeachings.AnyAsync(mt =>
            mt.ActionId == existingAction.Id
            && mt.ModuleId == existingModule.Id
            && mt.Id != entityDto.Id);
        if (alreadyTeacherAssociated)
        {
            _logger.LogWarning("Another teacher already associated with Module {ModuleId} in Action {ActionId} during update.", entityDto.ModuleId, entityDto.ActionId);
            return Result<RetrieveModuleTeachingDto>
                .Fail("Erro de Validação.",
                "Já existe outro Formador associado a este Módulo nesta Ação.");
        }

        // Update the entity
        existingModuleTeaching.TeacherId = entityDto.TeacherId;
        existingModuleTeaching.ModuleId = entityDto.ModuleId;
        existingModuleTeaching.ActionId = entityDto.ActionId;
        existingModuleTeaching.Teacher = existingTeacher;
        existingModuleTeaching.Module = existingModule;
        existingModuleTeaching.Action = existingAction;
        existingModuleTeaching.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var retrieveModuleTeaching = ModuleTeaching.ConvertEntityToRetrieveDto(existingModuleTeaching);

        _logger.LogInformation("ModuleTeaching with ID {Id} updated successfully. Teacher {TeacherId} associated with Module {ModuleId} in Action {ActionId}.", entityDto.Id, entityDto.TeacherId, entityDto.ModuleId, entityDto.ActionId);

        return Result<RetrieveModuleTeachingDto>
            .Ok(retrieveModuleTeaching, "Associação atualizada.",
            $"A associação foi atualizada com sucesso para a Ação Formação {existingAction.ActionNumber} - {existingAction.Locality}.");
    }
}
