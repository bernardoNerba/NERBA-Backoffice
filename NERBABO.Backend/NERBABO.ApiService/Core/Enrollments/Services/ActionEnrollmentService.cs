using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Enrollments.Dtos;
using NERBABO.ApiService.Core.Enrollments.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Helper;
using Humanizer;

namespace NERBABO.ApiService.Core.Enrollments.Services;

public class ActionEnrollmentService(
    AppDbContext context,
    ILogger<ActionEnrollment> logger
) : IActionEnrollmentService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<ActionEnrollment> _logger = logger;

    public async Task<Result<RetrieveActionEnrollmentDto>> CreateAsync(CreateActionEnrollmentDto entityDto)
    {
        var existingAction = await _context.Actions
            .AsNoTracking()
            .Include(a => a.Course).ThenInclude(c => c.Modules)
            .Include(a => a.ModuleTeachings)
            .FirstOrDefaultAsync(a => a.Id == entityDto.ActionId);
        if (existingAction is null)
        {
            _logger.LogWarning("Action with ID {ActionId} not found during Action enrollment creation.", entityDto.ActionId);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Não encontrado.", "Ação não encontrada.",
                StatusCodes.Status404NotFound);
        }

        var existingStudent = await _context.Students
            .AsNoTracking()
            .Include(s => s.Person)
            .FirstOrDefaultAsync(s => s.Id == entityDto.StudentId);
        if (existingStudent is null)
        {
            _logger.LogWarning("Student with ID {StudentId} not found during Action enrollment creation.", entityDto.StudentId);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Não encontrado.", "Formando não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // Check if the all modules of the action have teacher
        if (!existingAction.AllModulesOfActionHaveTeacher)
        {
            _logger.LogWarning("Action {ActionId} has modules without assigned teachers. Cannot create Action enrollment for student {StudentId}.", 
                existingAction.Id, entityDto.StudentId);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Erro de Validação.", "Falta atribuir Formador a um ou mais Módulos.");
        }

        // Verify if the student is already registered on this Action
        var isStudentAlreadyEnrolled = await _context.ActionEnrollments
            .Where(ae => ae.ActionId == existingAction.Id)
            .AnyAsync(ae => ae.StudentId == existingStudent.Id);
        if (isStudentAlreadyEnrolled)
        {
            _logger.LogWarning("Student {StudentId} is already enrolled in action {ActionId}. Duplicate enrollment attempt blocked.", 
                entityDto.StudentId, entityDto.ActionId);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Erro de Validação.", "O Formando já está inscrito nesta ação.");
        }

        // Get fresh tracked entities for the converter to avoid EF conflicts
        var trackedAction = await _context.Actions.FindAsync(entityDto.ActionId);
        var trackedStudent = await _context.Students.FindAsync(entityDto.StudentId);

        var newEnrollment = ActionEnrollment.ConvertCreateDtoToEntity(entityDto, trackedAction!, trackedStudent!);

        RetrieveActionEnrollmentDto retrieveAE;
        try
        {
            var createdEntity = _context.ActionEnrollments.Add(newEnrollment);
            await _context.SaveChangesAsync();

            // Reload the entity with navigation properties for the response
            var savedEnrollment = await _context.ActionEnrollments
                .Include(ae => ae.Student).ThenInclude(s => s.Person)
                .Include(ae => ae.Action)
                .FirstAsync(ae => ae.Id == createdEntity.Entity.Id);

            retrieveAE = ActionEnrollment.ConvertEntityToRetrieveDto(savedEnrollment);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating Action enrollment for Student {StudentId} in Action {ActionId}", 
                entityDto.StudentId, entityDto.ActionId);
            
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Erro de Base de Dados.", "Erro ao criar inscrição. Possível duplicação ou violação de restrições.");
        }

        _logger.LogInformation("{entity} created successfully. Student {StudentId} enrolled on {ActionId}.",
            nameof(ActionEnrollment), entityDto.StudentId, entityDto.ActionId);

        return Result<RetrieveActionEnrollmentDto>
                .Ok(retrieveAE, "Inscrito com sucesso.",
                $"O formando {existingStudent.Person.FullName} foi inscrito na ação {existingAction.Title}.",
                StatusCodes.Status201Created);
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var existingEnrollment = await _context.ActionEnrollments
            .FirstOrDefaultAsync(ae => ae.Id == id);
        
        if (existingEnrollment is null)
        {
            _logger.LogWarning("Action enrollment with ID {EnrollmentId} not found during deletion.", id);
            return Result.Fail("Não encontrado.", "Inscrição de ação não encontrada.",
                StatusCodes.Status404NotFound);
        }

        try
        {
            _context.ActionEnrollments.Remove(existingEnrollment);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting Action enrollment {EnrollmentId}", id);
            return Result.Fail("Erro de Base de Dados.", "Erro ao eliminar inscrição.");
        }

        _logger.LogInformation("Action enrollment {EnrollmentId} deleted successfully.", id);

        return Result.Ok("Eliminado com sucesso.", "A inscrição foi eliminada com sucesso.",
            StatusCodes.Status200OK);
    }

    public async Task<Result<IEnumerable<RetrieveActionEnrollmentDto>>> GetAllAsync()
    {
        var actionEnrollments = await _context.ActionEnrollments
            .AsNoTracking()
            .Include(e => e.Student).ThenInclude(s => s.Person)
            .Include(e => e.Action)
            .ToListAsync()
            ?? [];
        
        var retrieveAEs = actionEnrollments.Select(ActionEnrollment.ConvertEntityToRetrieveDto);

        return Result<IEnumerable<RetrieveActionEnrollmentDto>>
                .Ok(retrieveAEs);
    }

    public async Task<Result<IEnumerable<RetrieveActionEnrollmentDto>>> GetAllByActionId(long actionId)
    {
        var existingAction = await _context.Actions
            .FirstOrDefaultAsync(a => a.Id == actionId);
        if (existingAction is null)
        {
            _logger.LogWarning("Action with ID {ActionId} not found when retrieving enrollments.", actionId);
            return Result<IEnumerable<RetrieveActionEnrollmentDto>>
                .Fail("Não encontrado.", "Ação não encontrada.",
                StatusCodes.Status404NotFound);
        }

        var actionEnrollments = await _context.ActionEnrollments
            .AsNoTracking()
            .Include(e => e.Student).ThenInclude(s => s.Person)
            .Include(e => e.Action)
            .Where(e => e.ActionId == actionId)
            .ToListAsync()
            ?? [];
        
        var retrieveAEs = actionEnrollments.Select(ActionEnrollment.ConvertEntityToRetrieveDto);

        return Result<IEnumerable<RetrieveActionEnrollmentDto>>
                .Ok(retrieveAEs);
    }

    public async Task<Result<RetrieveActionEnrollmentDto>> GetByIdAsync(long id)
    {
        var enrollment = await _context.ActionEnrollments
            .AsNoTracking()
            .Include(e => e.Student).ThenInclude(s => s.Person)
            .Include(e => e.Action)
            .FirstOrDefaultAsync(e => e.Id == id);
        
        if (enrollment is null)
        {
            _logger.LogWarning("Action enrollment with ID {EnrollmentId} not found.", id);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Não encontrado.", "Inscrição de ação não encontrada.",
                StatusCodes.Status404NotFound);
        }
        
        var retrieveDto = ActionEnrollment.ConvertEntityToRetrieveDto(enrollment);

        return Result<RetrieveActionEnrollmentDto>
                .Ok(retrieveDto);
    }

    public async Task<Result<RetrieveActionEnrollmentDto>> UpdateAsync(UpdateActionEnrollmentDto entityDto)
    {
        var existingEnrollment = await _context.ActionEnrollments
            .Include(ae => ae.Student).ThenInclude(s => s.Person)
            .Include(ae => ae.Action)
            .FirstOrDefaultAsync(ae => ae.Id == entityDto.Id);
        
        if (existingEnrollment is null)
        {
            _logger.LogWarning("Action enrollment with ID {EnrollmentId} not found during update.", entityDto.Id);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Não encontrado.", "Inscrição de ação não encontrada.",
                StatusCodes.Status404NotFound);
        }

        await _context.SaveChangesAsync();
        var retrieveDto = ActionEnrollment.ConvertEntityToRetrieveDto(existingEnrollment);

        _logger.LogInformation("Action enrollment {EnrollmentId} updated successfully.", entityDto.Id);

        return Result<RetrieveActionEnrollmentDto>
            .Ok(retrieveDto, "Atualizado com sucesso.",
            "A inscrição foi atualizada com sucesso.",
            StatusCodes.Status200OK);
    }
}