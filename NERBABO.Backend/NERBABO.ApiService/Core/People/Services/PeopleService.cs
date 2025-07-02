using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Helper;
using NERBABO.ApiService.Shared.Enums;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;
using ZLinq;

namespace NERBABO.ApiService.Core.People.Services;

public class PeopleService(
        ILogger<PeopleService> logger,
        AppDbContext context,
        UserManager<User> userManager,
        ICacheService cacheService
    ) : IPeopleService
{
    private readonly ILogger<PeopleService> _logger = logger;
    private readonly AppDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;
    private readonly ICacheService _cacheService = cacheService;

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
        // update cache
        var cache_key = $"person:{createdPerson.Entity.Id}";
        await _cacheService.SetAsync(cache_key, personToRetrieve, TimeSpan.FromMinutes(30));
        await DeletePeopleCacheAsync();

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

        // Remove from cache
        await DeletePeopleCacheAsync(id);

        return Result
            .Ok("Pessoa Eliminada.", "Pessoa eliminada com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrievePersonDto>>> GetAllAsync()
    {
        // Check if entry exists in cache
        var cacheKey = "person:list";
        var cachedPeople = await _cacheService.GetAsync<List<RetrievePersonDto>>(cacheKey);
        if (cachedPeople is not null && cachedPeople.Count > 0)
            return Result<IEnumerable<RetrievePersonDto>>
                .Ok(cachedPeople);

        // Not in cache so fetch from database
        var existingPeople = _context.People
            .Include(p => p.User)
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
        await _cacheService.SetAsync(cacheKey, existingPeople, TimeSpan.FromMinutes(30));

        return Result<IEnumerable<RetrievePersonDto>>
            .Ok(existingPeople);
    }

    public async Task<Result<IEnumerable<RetrievePersonDto>>> GetAllWithoutUserAsync()
    {
        // Check if data entries exist in cache
        var cacheKey = "person:without:user";
        var cachedPeople = await _cacheService.GetAsync<List<RetrievePersonDto>>(cacheKey);
        if (cachedPeople is not null && cachedPeople.Count > 0)
            return Result<IEnumerable<RetrievePersonDto>>
                .Ok(cachedPeople);

        var existingUsers = await _context.Users.ToListAsync();
        var existingPeople = await _context.People.ToListAsync();

        var userIdsWithPeople = existingUsers
            .AsValueEnumerable()
            .Select(u => u.PersonId)
            .ToHashSet();

        var peopleWithoutUser = existingPeople
            .AsValueEnumerable()
            .Where(p => !userIdsWithPeople.Contains(p.Id))
            .Select(Person.ConvertEntityToRetrieveDto)
            .ToList();

        // Check if data
        if (peopleWithoutUser is null || peopleWithoutUser.Count == 0)
            return Result<IEnumerable<RetrievePersonDto>>
                .Fail("Não encontrado.", "Não foram encontradas pessoas sem conta registada no sistema",
                StatusCodes.Status404NotFound);

        await _cacheService.SetAsync(cacheKey, peopleWithoutUser, TimeSpan.FromMinutes(30));

        return Result<IEnumerable<RetrievePersonDto>>
            .Ok(peopleWithoutUser);
    }

    public async Task<Result<RetrievePersonDto>> GetByIdAsync(long id)
    {
        // Check if entry exists in cache
        var cacheKey = $"person:{id}";
        var cachedPerson = await _cacheService.GetAsync<RetrievePersonDto>(cacheKey);
        if (cachedPerson is not null)
            return Result<RetrievePersonDto>.Ok(cachedPerson);

        // Not in cache so fetch from database
        var existingPerson = await _context.People
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

        if (existingPerson is null)
            return Result<RetrievePersonDto>
                .Fail("Não encontrado.", "Pessoa não encontrada.",
                StatusCodes.Status404NotFound);

        // update cache
        await _cacheService.SetAsync(cacheKey, Person.ConvertEntityToRetrieveDto(existingPerson), TimeSpan.FromMinutes(30));

        return Result<RetrievePersonDto>.Ok(Person.ConvertEntityToRetrieveDto(existingPerson));
    }

    public async Task<Result<RetrievePersonDto>> UpdateAsync(UpdatePersonDto entityDto)
    {
        var existingPerson = await _context.People.FindAsync(entityDto.Id);
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

        // Update cache
        var cacheKey = $"person:{existingPerson.Id}";
        await _cacheService.SetAsync(cacheKey, Person.ConvertEntityToRetrieveDto(existingPerson), TimeSpan.FromMinutes(30));
        await DeletePeopleCacheAsync();

        return Result<RetrievePersonDto>
            .Ok(Person.ConvertEntityToRetrieveDto(existingPerson),
                "Pessoa Atualizada.",
                $"Foi atualizada a pessoa com o nome {existingPerson.FirstName} {existingPerson.LastName}.");
    }

    private async Task DeletePeopleCacheAsync(long? id = null)
    {
        if (id is not null)
            await _cacheService.RemoveAsync($"person:{id}");

        await _cacheService.RemoveAsync("person:list");
        await _cacheService.RemoveAsync("person:without:user");
    }
}