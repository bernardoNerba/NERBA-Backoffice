using System;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Enrollments.Dtos;
using NERBABO.ApiService.Core.Enrollments.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using OpenTelemetry.Trace;

namespace NERBABO.ApiService.Core.Enrollments.Services;

public class MTEnrollmentService(
    AppDbContext context,
    ILogger<MTEnrollment> logger
) : IMTEnrollmentService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<MTEnrollment> _logger = logger;

    public async Task<Result<RetrieveMTEnrollmentDto>> CreateAsync(CreateMTEnrollmentDto entityDto)
    {
        var existingAction = await _context.Actions
            .AsNoTracking()
            .Include(a => a.Course).ThenInclude(c => c.Modules)
            .Include(a => a.ModuleTeachings)
            .FirstOrDefaultAsync(a => a.Id == entityDto.ActionId);
        if (existingAction is null)
        {
            _logger.LogWarning("Action with ID {ActionId} not found during MT enrollment creation.", entityDto.ActionId);
            return Result<RetrieveMTEnrollmentDto>
                .Fail("Não encontrado.", "Relação Modulo Ação Formador não encontrada.",
                StatusCodes.Status404NotFound);
        }

        var existingStudent = await _context.Students
            .AsNoTracking()
            .Include(s => s.Person)
            .FirstOrDefaultAsync(s => s.Id == entityDto.StudentId);
        if (existingStudent is null)
        {
            _logger.LogWarning("Student with ID {StudentId} not found during MT enrollment creation.", entityDto.StudentId);
            return Result<RetrieveMTEnrollmentDto>
                .Fail("Não encontrado.", "Formando não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // Check if the all modules of the action have teacher
        if (!existingAction.AllModulesOfActionHaveTeacher)
        {
            _logger.LogWarning("Action {ActionId} has modules without assigned teachers. Cannot create MT enrollment for student {StudentId}.", 
                existingAction.Id, entityDto.StudentId);
            return Result<RetrieveMTEnrollmentDto>
                .Fail("Erro de Validação.", "Falta atribuir Formador a um ou mais Módulos.");
        }

        // Verify if the student is already registered on this ModuleTeaching
        var isStudentAlreadyEnrolled = await _context.MTEnrollments
            .Where(mte => mte.ActionId == existingAction.Id)
            .AnyAsync(mte => mte.StudentId == existingStudent.Id);
        if (isStudentAlreadyEnrolled)
        {
            _logger.LogWarning("Student {StudentId} is already enrolled in action {ActionId}. Duplicate enrollment attempt blocked.", 
                entityDto.StudentId, entityDto.ActionId);
            return Result<RetrieveMTEnrollmentDto>
                .Fail("Erro de Validação.", "O Formando já está inscrito nesta ação.");
        }

        // Get fresh tracked entities for the converter to avoid EF conflicts
        var trackedAction = await _context.Actions.FindAsync(entityDto.ActionId);
        var trackedStudent = await _context.Students.FindAsync(entityDto.StudentId);

        var newEnrollment = MTEnrollment.ConvertCreateDtoToEntity(entityDto, trackedAction!, trackedStudent!);

        RetrieveMTEnrollmentDto retrieveMTE;
        try
        {
            var createdEntity = _context.MTEnrollments.Add(newEnrollment);
            await _context.SaveChangesAsync();

            // Reload the entity with navigation properties for the response
            var savedEnrollment = await _context.MTEnrollments
                .Include(mte => mte.Student).ThenInclude(s => s.Person)
                .Include(mte => mte.Action)
                .FirstAsync(mte => mte.Id == createdEntity.Entity.Id);

            retrieveMTE = MTEnrollment.ConvertEntityToRetrieveDto(savedEnrollment);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating MT enrollment for Student {StudentId} in Action {ActionId}", 
                entityDto.StudentId, entityDto.ActionId);
            
            return Result<RetrieveMTEnrollmentDto>
                .Fail("Erro de Base de Dados.", "Erro ao criar inscrição. Possível duplicação ou violação de restrições.");
        }

        _logger.LogInformation("{entity} created successfully. Student {StudentId} enrolled on {ActionId}.",
            nameof(MTEnrollment), entityDto.StudentId, entityDto.ActionId);

        return Result<RetrieveMTEnrollmentDto>
                .Ok(retrieveMTE, "Inscrito com sucesso.",
                $"O formando {existingStudent.Person.FullName} foi inscrito na ação {existingAction.Title}.",
                StatusCodes.Status201Created);
    }

    public Task<Result> DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<RetrieveMTEnrollmentDto>>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result<RetrieveMTEnrollmentDto>> GetByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<Result<RetrieveMTEnrollmentDto>> UpdateAsync(UpdateMTEnrollmentDto entityDto)
    {
        throw new NotImplementedException();
    }
}
