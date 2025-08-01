using System.Linq.Expressions;
using Humanizer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.People.Cache;
using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Core.Students.Cache;
using NERBABO.ApiService.Core.Teachers.Cache;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.People.Services;

public class PeopleService(
        ILogger<PeopleService> logger,
        AppDbContext context,
        UserManager<User> userManager,
        ICachePeopleRepository cache,
        ICacheStudentsRepository cacheStudents,
        ICacheTeacherRepository cacheTeacher

    ) : IPeopleService
{
    private readonly ILogger<PeopleService> _logger = logger;
    private readonly AppDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ICachePeopleRepository _cache = cache;
    private readonly ICacheStudentsRepository _cacheStudents = cacheStudents;
    private readonly ICacheTeacherRepository _cacheTeacher = cacheTeacher;

    public async Task<Result<RetrievePersonDto>> CreateAsync(CreatePersonDto entityDto)
    {
        // Unique constraints validation
        if (await _context.People.AnyAsync(p => p.NIF == entityDto.NIF))
        {
            _logger.LogWarning("Duplicated NIF try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O NIF da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(entityDto.NISS) &&
            await _context.People.AnyAsync(p => p.NISS == entityDto.NISS))
        {
            _logger.LogWarning("Duplicated NISS try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O NISS da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(entityDto.IdentificationNumber)
            && await _context.People.AnyAsync(p =>
            (p.IdentificationNumber ?? "").ToLower()
            .Equals(entityDto.IdentificationNumber.ToLower()))
            )
        {
            _logger.LogWarning("Duplicated IdentificationNumber try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O Número de Identificação da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(entityDto.Email)
            && await _context.People.AnyAsync(p =>
            (p.Email ?? "").ToLower()
            .Equals(entityDto.Email.ToLower()))
            )
        {
            _logger.LogWarning("Duplicated Email try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O Email da pessoa deve ser único. Já existe no sistema.");
        }

        // Enum validation
        if (string.IsNullOrEmpty(entityDto.Gender)
            || !EnumHelp.IsValidEnum<GenderEnum>(entityDto.Gender))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Género não encontrado.",
                StatusCodes.Status404NotFound);
        }

        if (string.IsNullOrEmpty(entityDto.Habilitation)
            || !EnumHelp.IsValidEnum<HabilitationEnum>(entityDto.Habilitation))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Tipo de Habilitações não encontrado.",
                StatusCodes.Status404NotFound);
        }

        if (string.IsNullOrEmpty(entityDto.IdentificationType)
            || !EnumHelp.IsValidEnum<IdentificationTypeEnum>(entityDto.IdentificationType))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Tipo de Identificação não encontrado.",
                StatusCodes.Status404NotFound);
        }

        // create person on database
        var createdPerson = _context.People.Add(Person.ConvertCreateDtoToEntity(entityDto));
        await _context.SaveChangesAsync();

        var personToRetrieve = Person.ConvertEntityToRetrieveDto(createdPerson.Entity);

        // Update cache
        await RemoveRelatedCache();
        await _cache.SetSinglePersonCacheAsync(personToRetrieve);


        return Result<RetrievePersonDto>
            .Ok(personToRetrieve,
            "Pessoa Criada.", $"Foi criada uma pessoa com o nome {personToRetrieve.FullName}.",
            StatusCodes.Status201Created);
    }

    public async Task<Result> DeleteAsync(long id)
    {
        // Check if person exists
        var existingPerson = await _context.People.FindAsync(id);
        if (existingPerson is null)
            return Result
                .Fail("Não encontrado.", $"Pessoa não encontrada.",
                StatusCodes.Status404NotFound);

        // Check if person is associated with a user
        if (await _userManager.Users.AnyAsync(u => u.PersonId == id))
        {
            _logger.LogWarning("Duplicated User association detected.");
            return Result
                .Fail("Erro de Validação.", "Não pode eliminar uma pessoa que é um utilizador.");
        }

        var transaction = _context.Database.BeginTransaction();
        try
        {
            var existingTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.PersonId == id);
            if (existingTeacher is not null)
            {
                _context.Teachers.Remove(existingTeacher);
            }

            var existingStudent = await _context.Students.FirstOrDefaultAsync(t => t.PersonId == id);
            if (existingStudent is not null)
            {
                _context.Students.Remove(existingStudent);
            }

            // remove from database
            _context.People.Remove(existingPerson);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            await transaction.CommitAsync();
        }

        // Update cache
        await RemoveRelatedCache(id);

        return Result
            .Ok("Pessoa Eliminada.", "Pessoa eliminada com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrievePersonDto>>> GetAllAsync()
    {
        // Check if entry exists in cache
        var cachedPeople = await _cache.GetCacheAllPeopleAsync();
        if (cachedPeople is not null && cachedPeople.ToList().Count > 0)
            return Result<IEnumerable<RetrievePersonDto>>
                .Ok(cachedPeople);

        // Not in cache so fetch from database
        var existingPeople = _context.People
            .Include(p => p.User)
            .Include(p => p.Teacher)
            .Include(p => p.Student)
            .Select(p => Person.ConvertEntityToRetrieveDto(p))
            .AsValueEnumerable()
            .OrderByDescending(p => p.FullName)
            .ToList();

        // Check if data
        if (existingPeople is null || existingPeople.Count == 0)
            return Result<IEnumerable<RetrievePersonDto>>
                .Fail("Não encontrado.", "Não foram encontradas pessoas no sistema",
                StatusCodes.Status404NotFound);

        // update cache
        await _cache.SetAllPeopleCacheAsync(existingPeople);

        return Result<IEnumerable<RetrievePersonDto>>
            .Ok(existingPeople);
    }

    public async Task<Result<IEnumerable<RetrievePersonDto>>> GetAllWithoutProfileAsync(string profile)
    {
        // Validate and parse the profile input
        if (string.IsNullOrWhiteSpace(profile) || !Enum.TryParse<ProfilesEnum>(profile.Humanize(LetterCasing.Title), true, out var profileEnum))
        {
            return Result<IEnumerable<RetrievePersonDto>>
                .Fail("Perfil inválido.", $"O perfil '{profile}' não é válido.", StatusCodes.Status400BadRequest);
        }

        // Check if data entries exist in cache
        var cachedPeople = await _cache.GetCachePeopleWithoutProfileAsync(profile);
        if (cachedPeople is not null && cachedPeople.ToList().Count > 0)
        {
            return Result<IEnumerable<RetrievePersonDto>>
                .Ok(cachedPeople);
        }

        // Fetch from database
        var existingPeople = await _context.People
            .Include(p => p.User)
            .Include(p => p.Teacher)
            .Include(p => p.Student)
            .ToListAsync();

        // Filter people without the specified profile
        Expression<Func<Person, bool>> filter = profileEnum switch
        {
            ProfilesEnum.Colaborator => p => !p.IsColaborator,
            ProfilesEnum.Student => p => !p.IsStudent,
            ProfilesEnum.Teacher => p => !p.IsTeacher,
            _ => throw new ArgumentOutOfRangeException(nameof(profileEnum), "Perfil não suportado.")
        };

        var peopleWithoutProfile = existingPeople
            .AsValueEnumerable()
            .Where(filter.Compile())
            .OrderByDescending(p => p.CreatedAt)
            .Select(Person.ConvertEntityToRetrieveDto)
            .ToList();

        // Check if data exists
        if (peopleWithoutProfile.Count == 0)
        {
            return Result<IEnumerable<RetrievePersonDto>>
                .Fail("Não encontrado.", $"Não foram encontradas pessoas sem o perfil {profile}.", StatusCodes.Status404NotFound);
        }

        // Update cache
        await _cache.SetPeopleWithoutProfileCacheAsync(peopleWithoutProfile, profile);

        return Result<IEnumerable<RetrievePersonDto>>.Ok(peopleWithoutProfile);
    }

    public async Task<Result<RetrievePersonDto>> GetByIdAsync(long id)
    {
        // Check if entry exists in cache
        var cachedPerson = await _cache.GetSinglePersonCacheAsync(id);
        if (cachedPerson is not null)
            return Result<RetrievePersonDto>.Ok(cachedPerson);

        // Not in cache so fetch from database
        var existingPerson = await _context.People
                .Include(p => p.User)
                .Include(p => p.Teacher)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

        if (existingPerson is null)
            return Result<RetrievePersonDto>
                .Fail("Não encontrado.", "Pessoa não encontrada.",
                StatusCodes.Status404NotFound);

        var retrievePerson = Person.ConvertEntityToRetrieveDto(existingPerson);

        // update cache
        await _cache.SetSinglePersonCacheAsync(retrievePerson);

        return Result<RetrievePersonDto>
            .Ok(Person.ConvertEntityToRetrieveDto(existingPerson));
    }

    public async Task<Result<RelationshipPersonDto>> GetPersonRelationshipsAsync(long personId)
    {
        var existingPerson = await _context.People
            .Include(p => p.Teacher)
            .Include(p => p.User)
            .Include(p => p.Student)
            .FirstOrDefaultAsync(p => p.Id == personId);
        if (existingPerson is null)
            return Result<RelationshipPersonDto>
                .Fail("Não encontrado.", "Pessoa não encontrada.");

        var relationship = new RelationshipPersonDto(existingPerson);

        return Result<RelationshipPersonDto>
            .Ok(relationship);
    }

    public async Task<Result<RetrievePersonDto>> UpdateAsync(UpdatePersonDto entityDto)
    {
        var existingPerson = await _context.People
            .Include(p => p.Teacher)
            .Include(p => p.Student)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == entityDto.Id);
        if (existingPerson is null)
            return Result<RetrievePersonDto>
                .Fail("Não encontrado.", "Pessoa não encontrada.",
                StatusCodes.Status404NotFound);

        // Check for unique constraints
        if (await _context.People.AnyAsync(p => p.NIF == entityDto.NIF
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated NIF try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O NIF da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(entityDto.NISS)
            && await _context.People.AnyAsync(p => p.NISS == entityDto.NISS
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated NISS try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O NISS da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(entityDto.IdentificationNumber)
            && await _context.People.AnyAsync(p =>
            (p.IdentificationNumber ?? "").ToLower()
            .Equals(entityDto.IdentificationNumber)
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated IdentificationNumber try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O Numero de Identificação da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(entityDto.Email)
            && await _context.People.AnyAsync(p =>
            (p.Email ?? "").ToLower()
            .Equals(entityDto.Email.ToLower())
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated Email try detected.");
            return Result<RetrievePersonDto>
                .Fail("Erro de Validação.", "O Email da pessoa deve ser único. Já existe no sistema.");
        }

        // enum checks
        if (!string.IsNullOrEmpty(entityDto.Gender)
            && !EnumHelp.IsValidEnum<GenderEnum>(entityDto.Gender))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Género não encontrado",
                StatusCodes.Status404NotFound);
        }

        if (!string.IsNullOrEmpty(entityDto.Habilitation)
            && !EnumHelp.IsValidEnum<HabilitationEnum>(entityDto.Habilitation))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Tipo de Habilitações não encontrado",
                StatusCodes.Status404NotFound);
        }

        if (!string.IsNullOrEmpty(entityDto.IdentificationType)
            && !EnumHelp.IsValidEnum<IdentificationTypeEnum>(entityDto.IdentificationType))
        {
            return Result<RetrievePersonDto>
                .Fail("Não encontrado", "Tipo de Identificação não encontrado",
                StatusCodes.Status404NotFound);
        }

        _context.Entry(existingPerson).CurrentValues.SetValues(Person.ConvertUpdateDtoToEntity(entityDto));
        await _context.SaveChangesAsync();

        var retrievePerson = Person.ConvertEntityToRetrieveDto(existingPerson);

        // Update cache
        await RemoveRelatedCache(retrievePerson.Id);
        await _cache.SetSinglePersonCacheAsync(retrievePerson);


        return Result<RetrievePersonDto>
            .Ok(retrievePerson,
                "Pessoa Atualizada.",
                $"Foi atualizada a pessoa com o nome {existingPerson.FirstName} {existingPerson.LastName}.");
    }

    private async Task RemoveRelatedCache(long? id = null)
    {
        await _cache.RemovePeopleCacheAsync(id);
        await _cacheStudents.RemoveStudentsCacheAsync();
        await _cacheTeacher.RemoveTeacherCacheAsync();
    }
}