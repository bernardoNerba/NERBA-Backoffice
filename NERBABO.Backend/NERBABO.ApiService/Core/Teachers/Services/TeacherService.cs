using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Teachers.Services;

public class TeacherService(
    AppDbContext context,
    ILogger<TeacherService> logger,
    ICacheService cacheService
    ) : ITeacherService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<TeacherService> _logger = logger;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<Result<RetrieveTeacherDto>> CreateAsync(CreateTeacherDto entityDto)
    {
        // foreign key constrains check
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

        // Fields validation
        if (iva.Type != TaxEnum.IVA)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            return Result<RetrieveTeacherDto>
                .Fail("Erro de Validação.", "IVA regime devem ser do tipo correto.");
        }

        if (irs.Type != TaxEnum.IRS)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            return Result<RetrieveTeacherDto>
                .Fail("Erro de Validação.", "IRS regime devem ser do tipo correto.");
        }

        if (await _context.Teachers.AnyAsync(t => t.PersonId == entityDto.PersonId))
        {
            _logger.LogWarning("Teacher already exists for Person ID: {PersonId}", entityDto.PersonId);
            return Result<RetrieveTeacherDto>
                .Fail("Erro de Validação.", "Já existe um Formador associado a esta pessoa.");
        }

        if (await _context.Teachers.AnyAsync(t => t.Ccp == entityDto.Ccp))
        {
            _logger.LogWarning("Teacher already exists with CCP: {Ccp}", entityDto.Ccp);
            return Result<RetrieveTeacherDto>
                .Fail("Erro Validação.", "Já existe um Formador com este CCP.");
        }

        var teacher = Teacher.ConvertCreateDtoToEntity(entityDto, person, iva, irs);

        var result = _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        var retrieveTeacher = Teacher.ConvertEntityToRetrieveDto(result.Entity);

        // update cache
        await DeleteTeacherCacheAsync();
        await _cacheService.SetAsync($"teacher:{retrieveTeacher.Id}", retrieveTeacher, TimeSpan.FromMinutes(30));

        _logger.LogInformation("Teacher created successfully with ID: {Id}", teacher.Id);
        return Result<RetrieveTeacherDto>
            .Ok(retrieveTeacher,
            "Formador criado.", "Formador criado com sucesso.",
            StatusCodes.Status201Created);
    }

    public async Task<Result> DeleteAsync(long teacherId)
    {
        // TODO: Implemente deletion logic when ready
        var existingTeacher = await _context.Teachers.FindAsync(teacherId);
        if (existingTeacher is null)
        {
            _logger.LogWarning("Teacher not found for teacher id {id}", teacherId);
            return Result
                .Fail("Não encontrado", "Formador não encontraod.",
                StatusCodes.Status404NotFound);
        }

        _context.Teachers.Remove(existingTeacher);
        await _context.SaveChangesAsync();

        // update cache
        await DeleteTeacherCacheAsync(teacherId);

        return Result
            .Ok("Formador Eliminado.", "Foi eliminado um formador com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveTeacherDto>>> GetAllAsync()
    {
        var cacheKey = "teacher:list";
        var cachedTeachers = await _cacheService.GetAsync<List<RetrieveTeacherDto>>(cacheKey);
        if (cachedTeachers is not null && cachedTeachers.Count != 0)
            return Result<IEnumerable<RetrieveTeacherDto>>
                .Ok(cachedTeachers);

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

        await _cacheService.SetAsync(cacheKey, existingTeachers, TimeSpan.FromMinutes(30));

        return Result<IEnumerable<RetrieveTeacherDto>>
            .Ok(existingTeachers);
    }

    public async Task<Result<RetrieveTeacherDto>> GetByIdAsync(long id)
    {
        var cacheKey = $"teacher:{id}";
        var cacheTeacher = await _cacheService.GetAsync<RetrieveTeacherDto>(cacheKey);
        if (cacheTeacher is not null)
            return Result<RetrieveTeacherDto>
                .Ok(cacheTeacher);

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

        // update cache
        await _cacheService.SetAsync(cacheKey, existingTeacher, TimeSpan.FromMinutes(30));

        return Result<RetrieveTeacherDto>
            .Ok(existingTeacher);
    }

    public async Task<Result<RetrieveTeacherDto>> GetByPersonIdAsync(long personId)
    {
        var cacheKey = $"teacher:person:{personId}";
        var cachedTeacher = await _cacheService.GetAsync<RetrieveTeacherDto>(cacheKey);
        if (cachedTeacher is not null)
            return Result<RetrieveTeacherDto>
                .Ok(cachedTeacher);
        
        var person = await _context.People.FindAsync(personId);
        if (person is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado", "Pessoa não encontrada.",
                StatusCodes.Status404NotFound);

        var teacher = await _context.Teachers
            .Include(t => t.IvaRegime)
            .Include(t => t.IrsRegime)
            .Include(t => t.Person)
            .FirstOrDefaultAsync(t => t.PersonId == personId);

        if (teacher is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado", "Esta pessoa ainda não é um formador.",
                StatusCodes.Status404NotFound);

        var retrieveTeacher = Teacher.ConvertEntityToRetrieveDto(teacher);

        await _cacheService.SetAsync(cacheKey, retrieveTeacher, TimeSpan.FromMinutes(30));

        return Result<RetrieveTeacherDto>
            .Ok(retrieveTeacher);
    }

    public async Task<Result<RetrieveTeacherDto>> UpdateAsync(UpdateTeacherDto entityDto)
    {
        var existingTeacher = await _context.Teachers
            .Include(t => t.IvaRegime)
            .Include(t => t.IrsRegime)
            .Include(t => t.Person)
            .Where(t => t.Id == entityDto.Id)
            .FirstOrDefaultAsync();
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
                .Fail("Erro de Validação.", "IVA regime devem ser do tipo correto.");
        }

        if (irs.Type != TaxEnum.IRS)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            return Result<RetrieveTeacherDto>
                .Fail("Erro de Validação.", "IRS regime devem ser do tipo correto.");
        }

        if (await _context.Teachers.AnyAsync(t => t.PersonId == entityDto.PersonId && t.Id != entityDto.Id))
        {
            _logger.LogWarning("Teacher already exists for Person ID: {PersonId}", entityDto.PersonId);
            return Result<RetrieveTeacherDto>
                .Fail("Erro de Validação.", "Já existe um Formador associado a esta pessoa.");
        }

        if (await _context.Teachers.AnyAsync(t => t.Ccp == entityDto.Ccp && t.Id != entityDto.Id))
        {
            _logger.LogWarning("Teacher already exists with CCP: {Ccp}", entityDto.Ccp);
            return Result<RetrieveTeacherDto>
                .Fail("Erro de Validação.", "Já existe um Formador com este CCP.");
        }

        var teacher = Teacher.ConvertUpdateDtoToEntity(entityDto, person, iva, irs);

        _context.Entry(existingTeacher).CurrentValues.SetValues(teacher);
        _context.SaveChanges();

        var retrieveTeacher = Teacher.ConvertEntityToRetrieveDto(existingTeacher);

        // update cache
        await _cacheService.SetAsync($"teacher:{entityDto.Id}", retrieveTeacher, TimeSpan.FromMinutes(30));
        await DeleteTeacherCacheAsync();

        _logger.LogInformation("Teacher updated successfully with ID: {Id}", existingTeacher.Id);
        return Result<RetrieveTeacherDto>
            .Ok(retrieveTeacher,
            "Formador Atualizado.", "Foi atualizado o formador com sucesso.");

    }

    private async Task DeleteTeacherCacheAsync(long? id = null)
    {
        if (id is not null)
            await _cacheService.RemoveAsync($"teacher:{id}");

        await _cacheService.RemoveAsync("teacher:list");
        await _cacheService.RemovePatternAsync("teacher:person:*");
    }
}
