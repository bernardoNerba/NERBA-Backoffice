using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Enrollments.Dtos;
using NERBABO.ApiService.Core.Enrollments.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;

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

    public Task<Result> DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<RetrieveActionEnrollmentDto>>> GetAllAsync()
    {
        throw new NotImplementedException();
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

    public Task<Result<RetrieveActionEnrollmentDto>> GetByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<RetrieveActionEnrollmentDto>> UpdateAsync(UpdateActionEnrollmentDto entityDto)
    {
        throw new NotImplementedException();
    }
}