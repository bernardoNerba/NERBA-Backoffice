using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Teachers.Services;

public class TeacherService(
    AppDbContext context,
    ILogger<TeacherService> logger
    ) : ITeacherService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<TeacherService> _logger = logger;

    public async Task<Result<RetrieveTeacherDto>> CreateAsync(CreateTeacherDto entityDto)
    {
        var iva = await _context.Taxes.FindAsync(entityDto.IvaRegimeId);
        if (iva is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado.", "Regime de IVA não encontrado.",
                StatusCodes.Status404NotFound);

        var irs = await _context.Taxes.FindAsync(entityDto.IrsRegimeId);
        if (irs is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado.", "Regime de IRS não encontrado.",
                StatusCodes.Status404NotFound);

        var person = await _context.People.FindAsync(entityDto.PersonId);
        if (person is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado.", "Pessoa não encontrado.",
                StatusCodes.Status404NotFound);

        if (iva.Type != TaxEnum.IVA)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            return Result<RetrieveTeacherDto>
                .Fail("Erro Validação.", "IVA regime devem ser do tipo correto.");
        }

        if (irs.Type != TaxEnum.IRS)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            return Result<RetrieveTeacherDto>
                .Fail("Erro Validação.", "IRS regime devem ser do tipo correto.");
        }

        if (await _context.Teachers.AnyAsync(t => t.PersonId == entityDto.PersonId))
        {
            _logger.LogWarning("Teacher already exists for Person ID: {PersonId}", entityDto.PersonId);
            return Result<RetrieveTeacherDto>
                .Fail("Erro Validação.", "Já existe um Formador associado a esta pessoa.");
        }

        if (await _context.Teachers.AnyAsync(t => t.Ccp == entityDto.Ccp))
        {
            _logger.LogWarning("Teacher already exists with CCP: {Ccp}", entityDto.Ccp);
            return Result<RetrieveTeacherDto>
                .Fail("Erro Validação.", "Já existe um Formador com este CCP.");
        }

        var teacher = Teacher.ConvertCreateDtoToEntity(entityDto, person, iva, irs);

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Teacher created successfully with ID: {Id}", teacher.Id);
        return Result<RetrieveTeacherDto>
            .Ok(Teacher.ConvertEntityToRetrieveDto(teacher));
    }

    public async Task<Result> DeleteAsync(long teacherId)
    {
        // TODO: Implemente deletion logic when ready
        var existingTeacher = await _context.Teachers.FindAsync(teacherId);
        if (existingTeacher is null)
        {
            _logger.LogWarning("Teacher not found for teacher id {id}",teacherId);
            return Result
                .Fail("Não encontrado", "Formador não encontraod.",
                StatusCodes.Status404NotFound);
        }

        _context.Teachers.Remove(existingTeacher);
        await _context.SaveChangesAsync();
        return Result
            .Ok("Formador Eliminado.", "Foi eliminado um formador com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveTeacherDto>>> GetAllAsync()
    {
        var existingTeachers = await _context.Teachers
            .Include(t => t.IvaRegime)
            .Include(t => t.IrsRegime)
            .Include(t => t.Person)
            .Select(t => Teacher.ConvertEntityToRetrieveDto(t))
            .ToListAsync();

        if (existingTeachers is null || existingTeachers.Count == 0)
            return Result<IEnumerable<RetrieveTeacherDto>>
                .Fail("Não encontrado", "Não existem formadores.", 
                StatusCodes.Status404NotFound);

        return Result<IEnumerable<RetrieveTeacherDto>>
            .Ok(existingTeachers);
    }

    public async Task<Result<RetrieveTeacherDto>> GetByIdAsync(long id)
    {
        var existingTeacher = await _context.Teachers
            .Include(t => t.IvaRegime)
            .Include(t => t.IrsRegime)
            .Include(t => t.Person)
            .Where(t => t.Id == id)
            .Select(t => Teacher.ConvertEntityToRetrieveDto(t))
            .FirstOrDefaultAsync();

        if (existingTeacher is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado", "Formador não encontrado.",
                StatusCodes.Status404NotFound);

        return Result<RetrieveTeacherDto>
            .Ok(existingTeacher);
    }

    public async Task<Result<RetrieveTeacherDto>> GetByPersonIdAsync(long personId)
    {
        var person = await _context.People.FindAsync(personId);

        if (person is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado", "Formador não encontrado.", 
                StatusCodes.Status404NotFound);

        var teacher = await _context.Teachers
            .Include(t => t.IvaRegime)
            .Include(t => t.IrsRegime)
            .Include(t => t.Person)
            .FirstOrDefaultAsync(t => t.PersonId == personId);

        if (teacher is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado", "Formador não encontrado.", StatusCodes.Status404NotFound);

        return Result<RetrieveTeacherDto>
            .Ok(Teacher.ConvertEntityToRetrieveDto(teacher));
    }

    public async Task<Result<RetrieveTeacherDto>> UpdateAsync(UpdateTeacherDto entityDto)
    {
        var existingTeacher = _context.Teachers.Find(entityDto.Id);
        if (existingTeacher is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado.", "Formador não encontrado.",
                StatusCodes.Status404NotFound);

        var iva = await _context.Taxes.FindAsync(entityDto.IvaRegimeId);
        if (iva is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado.", "Regime de IVA não encontrado.",
                StatusCodes.Status404NotFound);

        var irs = await _context.Taxes.FindAsync(entityDto.IrsRegimeId);
        if (irs is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado.", "Regime de IRS não encontrado.",
                StatusCodes.Status404NotFound);

        var person = await _context.People.FindAsync(entityDto.PersonId);
        if (person is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado.", "Pessoa não encontrado.",
                StatusCodes.Status404NotFound);

        if (iva.Type != TaxEnum.IVA)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            return Result<RetrieveTeacherDto>
                .Fail("Erro Validação.", "IVA regime devem ser do tipo correto.");
        }

        if (irs.Type != TaxEnum.IRS)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            return Result<RetrieveTeacherDto>
                .Fail("Erro Validação.", "IRS regime devem ser do tipo correto.");
        }

        if (await _context.Teachers.AnyAsync(t => t.PersonId == entityDto.PersonId && t.Id != entityDto.Id))
        {
            _logger.LogWarning("Teacher already exists for Person ID: {PersonId}", entityDto.PersonId);
            return Result<RetrieveTeacherDto>
                .Fail("Erro Validação.", "Já existe um Formador associado a esta pessoa.");
        }

        if (await _context.Teachers.AnyAsync(t => t.Ccp == entityDto.Ccp && t.Id != entityDto.Id))
        {
            _logger.LogWarning("Teacher already exists with CCP: {Ccp}", entityDto.Ccp);
            return Result<RetrieveTeacherDto>
                .Fail("Erro Validação.", "Já existe um Formador com este CCP.");
        }

        var teacher = Teacher.ConvertUpdateDtoToEntity(entityDto, person, iva, irs);


        _context.Entry(existingTeacher).CurrentValues.SetValues(teacher);
        _context.SaveChanges();

        _logger.LogInformation("Teacher updated successfully with ID: {Id}", existingTeacher.Id);
        return Result<RetrieveTeacherDto>
            .Ok(Teacher.ConvertEntityToRetrieveDto(existingTeacher), 
            "Formador Atualizado.", "Foi atualizado o formador com sucesso.");

    }
}
