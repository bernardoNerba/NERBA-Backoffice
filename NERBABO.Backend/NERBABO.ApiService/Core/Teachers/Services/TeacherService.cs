using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.People.Cache;
using NERBABO.ApiService.Core.Teachers.Cache;
using NERBABO.ApiService.Core.Teachers.Dtos;
using NERBABO.ApiService.Core.Teachers.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;

namespace NERBABO.ApiService.Core.Teachers.Services;

public class TeacherService(
    AppDbContext context,
    ILogger<TeacherService> logger,
    ICacheTeacherRepository cache,
    ICachePeopleRepository cachePeople
    ) : ITeacherService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<TeacherService> _logger = logger;
    private readonly ICacheTeacherRepository _cache = cache;
    private readonly ICachePeopleRepository _cachePeople = cachePeople;

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
        await RemoveRelatedCache(retrieveTeacher.Id, retrieveTeacher.PersonId);
        await _cache.SetSingleTeacherCacheAsync(retrieveTeacher);

        _logger.LogInformation("Teacher created successfully with ID: {Id}", teacher.Id);
        return Result<RetrieveTeacherDto>
            .Ok(retrieveTeacher,
            "Formador criado.", "Formador criado com sucesso.",
            StatusCodes.Status201Created);
    }

    public async Task<Result> DeleteAsync(long teacherId)
    {
        var existingTeacher = await _context.Teachers.FindAsync(teacherId);
        if (existingTeacher is null)
        {
            _logger.LogWarning("Teacher not found for teacher id {id}", teacherId);
            return Result
                .Fail("Não encontrado", "Formador não encontraod.",
                StatusCodes.Status404NotFound);
        }

        // check if the teacher has records of module teaching
        var existsModuleTeaching = await _context.ModuleTeachings
            .Where(mt => mt.TeacherId == teacherId)
            .AnyAsync();
        if (existsModuleTeaching)
        {
            _logger.LogWarning("Teacher with ID {teacherId} has records related with module teaching.", teacherId);
            return Result
                .Fail("Erro de Validação", "Não é possível a eliminação, pois existem módulos em ações lecionadas por este formador.");
        }

        _context.Teachers.Remove(existingTeacher);
        await _context.SaveChangesAsync();

        // update cache
        await RemoveRelatedCache(teacherId, existingTeacher.PersonId);

        return Result
            .Ok("Formador Eliminado.", "Foi eliminado um formador com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveTeacherDto>>> GetAllAsync()
    {
        var cachedTeachers = await _cache.GetCacheAllTeacherAsync();
        if (cachedTeachers is not null && cachedTeachers.ToList().Count != 0)
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

        await _cache.SetAllTeacherCacheAsync(existingTeachers);

        return Result<IEnumerable<RetrieveTeacherDto>>
            .Ok(existingTeachers);
    }

    public async Task<Result<RetrieveTeacherDto>> GetByIdAsync(long id)
    {
        var cacheTeacher = await _cache.GetSingleTeacherCacheAsync(id);
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
        await _cache.SetSingleTeacherCacheAsync(existingTeacher);

        return Result<RetrieveTeacherDto>
            .Ok(existingTeacher);
    }

    public async Task<Result<RetrieveTeacherDto>> GetByPersonIdAsync(long personId)
    {
        // Check if the person exists
        var existingPerson = await _context.People
            .Include(p => p.Teacher)
            .FirstOrDefaultAsync(p => p.Id == personId);
        if (existingPerson is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado", "Pessoa não encontrada.",
                StatusCodes.Status404NotFound);

        // Check if the person is a teacher
        if (existingPerson.Teacher is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado", "Esta pessoa ainda não é um formador.",
                StatusCodes.Status404NotFound);

        // Check cache for teacher
        var cachedTeacher = await _cache.GetSingleTeacherCacheAsync(existingPerson.Teacher.Id);
        if (cachedTeacher is not null)
            return Result<RetrieveTeacherDto>
                .Ok(cachedTeacher);

        // not in cache so get the teacher data
        var teacher = await _context.Teachers
            .Include(t => t.IvaRegime)
            .Include(t => t.IrsRegime)
            .Include(t => t.Person)
            .FirstOrDefaultAsync(t => t.PersonId == personId);

        // checked above and this person id has indead a teacher associated
        var retrieveTeacher = Teacher.ConvertEntityToRetrieveDto(teacher!);

        // update cache
        await _cache.SetSingleTeacherCacheAsync(retrieveTeacher);

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

        // Check if IVA exists
        var iva = await _context.Taxes.FindAsync(entityDto.IvaRegimeId);
        if (iva is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado.", "Regime de IVA não encontrado.",
                StatusCodes.Status404NotFound);

        // Check if IRS exists
        var irs = await _context.Taxes.FindAsync(entityDto.IrsRegimeId);
        if (irs is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado.", "Regime de IRS não encontrado.",
                StatusCodes.Status404NotFound);

        // Check if related Person exists
        var person = await _context.People.FindAsync(entityDto.PersonId);
        if (person is null)
            return Result<RetrieveTeacherDto>
                .Fail("Não encontrado.", "Pessoa não encontrado.",
                StatusCodes.Status404NotFound);

        // Check if iva is a IVA type
        if (iva.Type != TaxEnum.IVA)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            return Result<RetrieveTeacherDto>
                .Fail("Erro de Validação.", "IVA regime devem ser do tipo correto.");
        }

        // check if irs is a IRS type
        if (irs.Type != TaxEnum.IRS)
        {
            _logger.LogError("Invalid tax types for Teacher creation.");
            return Result<RetrieveTeacherDto>
                .Fail("Erro de Validação.", "IRS regime devem ser do tipo correto.");
        }

        // check unique constraint with person
        if (await _context.Teachers.AnyAsync(t => t.PersonId == entityDto.PersonId && t.Id != entityDto.Id))
        {
            _logger.LogWarning("Teacher already exists for Person ID: {PersonId}", entityDto.PersonId);
            return Result<RetrieveTeacherDto>
                .Fail("Erro de Validação.", "Já existe um Formador associado a esta pessoa.");
        }

        // check if CCP is unique
        if (await _context.Teachers.AnyAsync(t => t.Ccp == entityDto.Ccp && t.Id != entityDto.Id))
        {
            _logger.LogWarning("Teacher already exists with CCP: {Ccp}", entityDto.Ccp);
            return Result<RetrieveTeacherDto>
                .Fail("Erro de Validação.", "Já existe um Formador com este CCP.");
        }

        // Selective field updates - only update fields that have changed
        bool hasChanges = false;

        // Update IvaRegimeId if changed
        if (existingTeacher.IvaRegimeId != entityDto.IvaRegimeId)
        {
            existingTeacher.IvaRegimeId = entityDto.IvaRegimeId;
            existingTeacher.IvaRegime = iva;
            hasChanges = true;
        }

        // Update IrsRegimeId if changed
        if (existingTeacher.IrsRegimeId != entityDto.IrsRegimeId)
        {
            existingTeacher.IrsRegimeId = entityDto.IrsRegimeId;
            existingTeacher.IrsRegime = irs;
            hasChanges = true;
        }

        // Update PersonId if changed
        if (existingTeacher.PersonId != entityDto.PersonId)
        {
            existingTeacher.PersonId = entityDto.PersonId;
            existingTeacher.Person = person;
            hasChanges = true;
        }

        // Update Ccp if changed
        if (!string.Equals(existingTeacher.Ccp, entityDto.Ccp))
        {
            existingTeacher.Ccp = entityDto.Ccp;
            hasChanges = true;
        }

        // Update Competences if changed
        if (!string.Equals(existingTeacher.Competences, entityDto.Competences))
        {
            existingTeacher.Competences = entityDto.Competences;
            hasChanges = true;
        }

        // Update IsLecturingFM if changed
        if (existingTeacher.IsLecturingFM != entityDto.IsLecturingFM)
        {
            existingTeacher.IsLecturingFM = entityDto.IsLecturingFM;
            hasChanges = true;
        }

        // Update IsLecturingCQ if changed
        if (existingTeacher.IsLecturingCQ != entityDto.IsLecturingCQ)
        {
            existingTeacher.IsLecturingCQ = entityDto.IsLecturingCQ;
            hasChanges = true;
        }

        // Return fail result if no changes were detected
        if (!hasChanges)
        {
            _logger.LogInformation("No changes detected for Teacher with ID {id}. No update performed.", entityDto.Id);
            return Result<RetrieveTeacherDto>
                .Fail("Nenhuma alteração detetada.", "Não foi alterado nenhum dado. Modifique os dados e tente novamente.",
                StatusCodes.Status400BadRequest);
        }

        // Update UpdatedAt and save changes
        existingTeacher.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var retrieveTeacher = Teacher.ConvertEntityToRetrieveDto(existingTeacher);

        // update cache
        await RemoveRelatedCache(retrieveTeacher.Id, retrieveTeacher.PersonId);
        await _cache.SetSingleTeacherCacheAsync(retrieveTeacher);

        _logger.LogInformation("Teacher updated successfully with ID: {Id}", existingTeacher.Id);
        return Result<RetrieveTeacherDto>
            .Ok(retrieveTeacher,
            "Formador Atualizado.", "Foi atualizado o formador com sucesso.");
    }

    private async Task RemoveRelatedCache(long? id = null, long? personId = null)
        {
            await _cache.RemoveTeacherCacheAsync(id);
            await _cachePeople.RemovePeopleCacheAsync(personId);
        }
}
