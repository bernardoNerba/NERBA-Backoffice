using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.People.Dtos;
using NERBABO.ApiService.Core.People.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Services;
using ZLinq;

namespace NERBABO.ApiService.Core.People.Services;

public class PeopleService : IPeopleService
{
    private readonly ILogger<PeopleService> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ICacheService _cacheService;

    public PeopleService(
        ILogger<PeopleService> logger,
        AppDbContext context,
        UserManager<User> userManager,
        ICacheService cacheService)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _cacheService = cacheService;
    }

    public async Task<RetrievePersonDto> CreatePersonAsync(CreatePersonDto person)
    {
        if (person == null)
        {
            _logger.LogError("CreatePersonAsync: person is null");
            throw new ArgumentNullException(nameof(person));
        }

        if (await _context.People.AnyAsync(p => p.NIF == person.NIF))
        {
            throw new Exception("O NIF da pessoa deve ser único. Já existe no sistema.");
        }

        if (await _context.People.AnyAsync(p => p.NISS == person.NISS)
            && !string.IsNullOrEmpty(person.NISS))
        {
            throw new Exception("O NISS da pessoa deve ser único. Já existe no sistema.");
        }

        if (await _context.People.AnyAsync(p => p.IdentificationNumber == person.IdentificationNumber)
            && !string.IsNullOrEmpty(person.IdentificationNumber))
        {
            throw new Exception("O Número de Identificação da pessoa deve ser único. Já existe no sistema.");
        }

        if (await _context.People.AnyAsync(p => p.Email == person.Email)
            && !string.IsNullOrEmpty(person.Email))
        {
            throw new Exception("O Email da pessoa deve ser único. Já existe no sistema.");
        }

        var newPerson = Person.ConvertCreateDtoToEntity(person);

        var createdPerson = _context.People.Add(newPerson);
        await _context.SaveChangesAsync();

        var cache_key = $"person:{createdPerson.Entity.Id}";

        await _cacheService.SetAsync(cache_key, Person.ConvertEntityToRetrieveDto(createdPerson.Entity), TimeSpan.FromMinutes(30));
        await _cacheService.RemoveAsync("people:list");

        return Person.ConvertEntityToRetrieveDto(newPerson);
    }

    public async Task<bool> DeletePersonAsync(long id)
    {

        var existingPerson = await _context.People
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingPerson == null)
        {
            throw new Exception("Pessoa não foi encontrada");
        }

        if (await _userManager.Users.Where(u => u.PersonId == id).AnyAsync())
        {
            throw new Exception("Não pode eliminar uma pessoa que é um utilizador.");
        }

        _context.Remove(existingPerson);
        await _context.SaveChangesAsync();

        await _cacheService.RemoveAsync($"person:{id}");
        await _cacheService.RemoveAsync("people:list");

        return true;
    }

    public async Task<IEnumerable<RetrievePersonDto>> GetAllPeopleAsync()
    {
        var cacheKey = "people:list";
        List<RetrievePersonDto> peopleToReturn = [];

        var cachedPeople = await _cacheService.GetAsync<List<RetrievePersonDto>>(cacheKey);

        if (cachedPeople != null && cachedPeople.Any())
        {
            peopleToReturn = cachedPeople;
        }
        else
        {
            var dbPeople = await _context.People
                .Include(p => p.User)
                .ToListAsync();

            foreach (var person in dbPeople)
            {
                peopleToReturn.Add(Person.ConvertEntityToRetrieveDto(person));
            }

            await _cacheService.SetAsync(cacheKey, peopleToReturn, TimeSpan.FromMinutes(30));
        }


        return [..peopleToReturn
                .AsValueEnumerable()
                .OrderByDescending(p => p.FullName)];
    }

    public async Task<RetrievePersonDto?> GetPersonByIdAsync(long id)
    {
        var cacheKey = $"person:{id}";

        var cachedPerson = await _cacheService.GetAsync<RetrievePersonDto>(cacheKey);
        if (cachedPerson != null)
        {
            return cachedPerson;
        }

        var dbPerson = await _context.People
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

        if (dbPerson == null)
        {
            _logger.LogWarning("GetPersonByIdAsync: Person not found");
            return null;
        }

        await _cacheService.SetAsync(cacheKey, Person.ConvertEntityToRetrieveDto(dbPerson), TimeSpan.FromMinutes(30));

        return Person.ConvertEntityToRetrieveDto(dbPerson);
    }

    public async Task<RetrievePersonDto?> UpdatePersonAsync(UpdatePersonDto person)
    {
        var existingPerson = await _context.People
                .FirstOrDefaultAsync(p => p.Id == person.Id);

        if (existingPerson == null)
        {
            _logger.LogWarning("UpdatePersonAsync: Person not found.");
            return null;
        }

        // Check for unique constraints
        if (await _context.People.AnyAsync(p => p.NIF == person.NIF
        && p.Id != existingPerson.Id))
        {
            throw new Exception("O NIF da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(person.NISS) &&
            await _context.People.AnyAsync(p => p.NISS == person.NISS && p.Id != existingPerson.Id))
        {
            throw new Exception("O NISS da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(person.IdentificationNumber) &&
            await _context.People.AnyAsync(p => p.IdentificationNumber == person.IdentificationNumber && p.Id != existingPerson.Id))
        {
            throw new Exception("O Número de Identificação da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(person.Email) &&
            await _context.People.AnyAsync(p => p.Email == person.Email && p.Id != existingPerson.Id))
        {
            throw new Exception("O Email da pessoa deve ser único. Já existe no sistema.");
        }

        _context.Entry(existingPerson).CurrentValues.SetValues(Person.ConvertUpdateDtoToEntity(person));

        await _context.SaveChangesAsync();

        var cacheKey = $"person:{existingPerson.Id}";
        await _cacheService.SetAsync(cacheKey, Person.ConvertEntityToRetrieveDto(existingPerson), TimeSpan.FromMinutes(30));
        await _cacheService.RemoveAsync("people:list");

        return Person.ConvertEntityToRetrieveDto(existingPerson);
    }
}