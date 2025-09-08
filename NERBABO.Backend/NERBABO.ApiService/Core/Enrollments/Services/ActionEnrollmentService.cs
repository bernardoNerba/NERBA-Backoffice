using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Enrollments.Dtos;
using NERBABO.ApiService.Core.Enrollments.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Core.SessionParticipations.Models;
using NERBABO.ApiService.Core.ModuleAvaliations.Models;
using NERBABO.ApiService.Shared.Enums;
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
        // Check if action exists
        var existingAction = await _context.Actions
            .Include(a => a.ModuleTeachings).ThenInclude(mt => mt.Module)
            .Include(a => a.ModuleTeachings).ThenInclude(mt => mt.Sessions)
            .FirstOrDefaultAsync(a => a.Id == entityDto.ActionId);
        if (existingAction is null)
        {
            _logger.LogWarning("Action with ID {ActionId} not found during Action enrollment creation.", entityDto.ActionId);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Não encontrado.", "Ação não encontrada.",
                StatusCodes.Status404NotFound);
        }

        // Check if student exists
        var existingStudent = await _context.Students
            .Include(s => s.Person)
            .FirstOrDefaultAsync(s => s.Id == entityDto.StudentId);
        if (existingStudent is null)
        {
            _logger.LogWarning("Student with ID {StudentId} not found during Action enrollment creation.", entityDto.StudentId);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Não encontrado.", "Formando não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // Check if all modules have teachers
        var hasAllTeachers = await _context.Actions
            .AsNoTracking()
            .Where(a => a.Id == entityDto.ActionId)
            .SelectMany(a => a.Course.Modules)
            .AllAsync(m => _context.ModuleTeachings
                .Any(mt => mt.ModuleId == m.Id && mt.ActionId == entityDto.ActionId));
        if (!hasAllTeachers)
        {
            _logger.LogWarning("Action {ActionId} has modules without assigned teachers. Cannot create Action enrollment for student {StudentId}.",
                entityDto.ActionId, entityDto.StudentId);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Erro de Validação.", "Falta atribuir Formador a um ou mais Módulos.");
        }

        // Check for duplicate enrollment
        var isStudentAlreadyEnrolled = await _context.ActionEnrollments
            .AsNoTracking()
            .AnyAsync(ae => ae.ActionId == entityDto.ActionId && ae.StudentId == entityDto.StudentId);
        if (isStudentAlreadyEnrolled)
        {
            _logger.LogWarning("Student {StudentId} is already enrolled in action {ActionId}. Duplicate enrollment attempt blocked.",
                entityDto.StudentId, entityDto.ActionId);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Erro de Validação.", "O Formando já está inscrito nesta ação.");
        }

        // Check if all Sessions are fully scheduled
        if (!existingAction.AllSessionsScheduled)
        {
            _logger.LogWarning("Not all sessions are scheduled accordingly, is not possible to add the new student {studentId} to the action {actionId}",
                entityDto.StudentId, entityDto.ActionId);
            return Result<RetrieveActionEnrollmentDto>
                .Fail("Erro de Validação.", "Faltam agendar sessões na ação.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Create ActionEnrollment
            var result = await _context.AddAsync(
                ActionEnrollment.ConvertCreateDtoToEntity(entityDto, existingAction, existingStudent)
                );
            var enrollment = result.Entity;

            // Create session participations relations
            var sessions = await _context.Sessions
                .Where(s => s.ModuleTeaching.ActionId == entityDto.ActionId)
                .ToListAsync();

            _logger.LogInformation("Found {Count} Sessions for Action {ActionId}", sessions.Count, entityDto.ActionId);

            var sessionParticipations = sessions
                .Select(session => new SessionParticipation
                {
                    SessionId = session.Id,
                    Session = session,
                    ActionEnrollmentId = enrollment.Id,
                    ActionEnrollment = enrollment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();

            // Create module avaliations relations
            var moduleTeachings = await _context.ModuleTeachings
                .Where(mt => mt.ActionId == entityDto.ActionId)
                .ToListAsync();

            _logger.LogInformation("Found {Count} ModuleTeachings for Action {ActionId}", moduleTeachings.Count, entityDto.ActionId);
            _logger.LogInformation("Using ActionEnrollmentId {EnrollmentId} for related entities", enrollment.Id);

            var moduleAvaliations = moduleTeachings
                .Select(moduleTeaching => new ModuleAvaliation
                {
                    ModuleTeachingId = moduleTeaching.Id,
                    ModuleTeaching = moduleTeaching,
                    ActionEnrollmentId = enrollment.Id,
                    ActionEnrollment = enrollment,
                    Grade = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToList();

            // Add all related entities
            if (sessionParticipations.Count > 0)
            {
                await _context.SessionParticipations.AddRangeAsync(sessionParticipations);
            }

            if (moduleAvaliations.Count > 0)
            {
                await _context.ModuleAvaliations.AddRangeAsync(moduleAvaliations);
            }

            // Save all changes in single transaction
            await _context.SaveChangesAsync();

            // Create response DTO with the data we already have
            var retrieveAE = new RetrieveActionEnrollmentDto
            {
                EnrollmentId = enrollment.Id,
                StudentFullName = existingStudent.Person.FullName,
                ApprovalStatus = ApprovalStatusEnum.NotSpecified.Humanize(LetterCasing.Title),
                ActionId = entityDto.ActionId,
                StudentAvaliated = false,
                AvgEvaluation = 0,
                PersonId = existingStudent.PersonId,
                StudentId = entityDto.StudentId,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("{entity} created successfully. Student {StudentId} enrolled on {ActionId}.",
                nameof(ActionEnrollment), entityDto.StudentId, entityDto.ActionId);

            return Result<RetrieveActionEnrollmentDto>
                    .Ok(retrieveAE, "Inscrito com sucesso.",
                    $"O formando {existingStudent.Person.FullName} foi inscrito na ação {existingAction.Title}.",
                    StatusCodes.Status201Created);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating Action enrollment for student {StudentId} in action {ActionId}.",
                entityDto.StudentId, entityDto.ActionId);

            return Result<RetrieveActionEnrollmentDto>
                .Fail("Erro interno do servidor.", "Erro ao criar inscrição. Tente novamente.",
                StatusCodes.Status500InternalServerError);
        }
        finally
        {
            await transaction.CommitAsync();
        }
    }

    public async Task<Result> DeleteAsync(long id)
    {
        var existingEnrollment = await _context.ActionEnrollments
            .Include(ae => ae.Student).ThenInclude(s => s.Person)
            .Include(ae => ae.Action)
            .FirstOrDefaultAsync(ae => ae.Id == id);

        if (existingEnrollment is null)
        {
            _logger.LogWarning("Action enrollment with ID {EnrollmentId} not found during deletion.", id);
            return Result.Fail("Não encontrado.", "Inscrição de ação não encontrada.",
                StatusCodes.Status404NotFound);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Get counts for logging purposes
            var sessionParticipationCount = await _context.SessionParticipations
                .CountAsync(sp => sp.ActionEnrollmentId == id);

            var moduleAvaliationCount = await _context.ModuleAvaliations
                .CountAsync(ma => ma.ActionEnrollmentId == id);

            // Remove the action enrollment (cascade delete will handle related entities)
            _context.ActionEnrollments.Remove(existingEnrollment);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Action enrollment {EnrollmentId} deleted successfully. " +
                "Cascade deleted {SessionParticipations} session participations and {ModuleAvaliations} module avaliations. " +
                "Student: {StudentName}, Action: {ActionTitle}",
                id, sessionParticipationCount, moduleAvaliationCount,
                existingEnrollment.Student.Person.FullName, existingEnrollment.Action.Title);

            return Result.Ok("Eliminado com sucesso.",
                $"A inscrição de {existingEnrollment.Student.Person.FullName} foi eliminada com sucesso.",
                StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex,
                "Error deleting Action enrollment {EnrollmentId} for student {StudentName} in action {ActionTitle}.",
                id, existingEnrollment.Student.Person.FullName, existingEnrollment.Action.Title);

            return Result.Fail("Erro interno do servidor.", "Erro ao eliminar inscrição. Tente novamente.",
                StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RetrieveActionEnrollmentDto>>> GetAllAsync()
    {
        var actionEnrollments = await _context.ActionEnrollments
            .AsNoTracking()
            .Include(e => e.Student).ThenInclude(s => s.Person)
            .Include(e => e.Action)
            .Include(e => e.Avaliations)
            .Include(e => e.Participants)
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
            .Include(e => e.Avaliations)
            .Include(e => e.Participants)
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
            .Include(e => e.Avaliations)
            .Include(e => e.Participants)
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