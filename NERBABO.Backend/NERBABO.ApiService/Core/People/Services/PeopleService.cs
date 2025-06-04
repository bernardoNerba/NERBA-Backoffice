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
        // Unique constraints checks
        if (await _context.People.AnyAsync(p => p.NIF == person.NIF))
        {
            _logger.LogWarning("Duplicated NIF try detected.");
            throw new InvalidOperationException("O NIF da pessoa deve ser único. Já existe no sistema.");
        }

        if (await _context.People.AnyAsync(p => p.NISS == person.NISS)
            && !string.IsNullOrEmpty(person.NISS))
        {
            _logger.LogWarning("Duplicated NISS try detected.");
            throw new InvalidOperationException("O NISS da pessoa deve ser único. Já existe no sistema.");
        }

        if (await _context.People.AnyAsync(p => p.IdentificationNumber == person.IdentificationNumber)
            && !string.IsNullOrEmpty(person.IdentificationNumber))
        {
            _logger.LogWarning("Duplicated IdentificationNumber try detected.");
            throw new InvalidOperationException("O Número de Identificação da pessoa deve ser único. Já existe no sistema.");
        }

        if (await _context.People.AnyAsync(p => p.Email == person.Email)
            && !string.IsNullOrEmpty(person.Email))
        {
            _logger.LogWarning("Duplicated Email try detected.");
            throw new InvalidOperationException("O Email da pessoa deve ser único. Já existe no sistema.");
        }

        // create person on database
        var createdPerson = _context.People.Add(Person.ConvertCreateDtoToEntity(person));
        await _context.SaveChangesAsync();

        var personToRetrieve = Person.ConvertEntityToRetrieveDto(createdPerson.Entity);
        // update cache
        var cache_key = $"person:{createdPerson.Entity.Id}";
        await _cacheService.SetAsync(cache_key, personToRetrieve, TimeSpan.FromMinutes(30));
        await _cacheService.RemoveAsync("people:list");

        return personToRetrieve;
    }

    public async Task DeletePersonAsync(long id)
    {
        // Check if person exists
        var existingPerson = await _context.People
            .FindAsync(id)
            ?? throw new KeyNotFoundException("Pessoa não encontrada.");

        // Check if person is associated with a user
        if (await _userManager.Users.Where(u => u.PersonId == id).AnyAsync())
        {
            _logger.LogWarning("Duplicated User association detected.");
            throw new InvalidOperationException("Não pode eliminar uma pessoa que é um utilizador.");
        }

        // remove from database
        _context.Remove(existingPerson);
        await _context.SaveChangesAsync();

        // Remove from cache
        await _cacheService.RemoveAsync($"person:{id}");
        await _cacheService.RemoveAsync("people:list");
    }

    public async Task<IEnumerable<RetrievePersonDto>> GetAllPeopleAsync()
    {
        // Check if entry exists in cache
        var cacheKey = "people:list";
        var cachedPeople = await _cacheService.GetAsync<List<RetrievePersonDto>>(cacheKey);
        if (cachedPeople != null && cachedPeople.Count > 0)
            return cachedPeople;

        // Not in cache so fetch from database
        var existingPeople = _context.People
            .Include(p => p.User)
            .Select(p => Person.ConvertEntityToRetrieveDto(p))
            .AsValueEnumerable()
            .OrderByDescending(p => p.FullName)
            .ToList();

        // update cache
        await _cacheService.SetAsync(cacheKey, existingPeople, TimeSpan.FromMinutes(30));

        return existingPeople;
    }

    public async Task<RetrievePersonDto> GetPersonByIdAsync(long id)
    {
        // Check if entry exists in cache
        var cacheKey = $"person:{id}";
        var cachedPerson = await _cacheService.GetAsync<RetrievePersonDto>(cacheKey);
        if (cachedPerson != null)
            return cachedPerson;

        // Not in cache so fetch from database
        var existingPeople = await _context.People
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw new KeyNotFoundException("Pessoa não encontrada");

        // update cache
        await _cacheService.SetAsync(cacheKey, Person.ConvertEntityToRetrieveDto(existingPeople), TimeSpan.FromMinutes(30));

        return Person.ConvertEntityToRetrieveDto(existingPeople);
    }

    public async Task<RetrievePersonDto> UpdatePersonAsync(UpdatePersonDto person)
    {
        var existingPerson = await _context.People.FindAsync(person.Id)
            ?? throw new KeyNotFoundException("Pessoa não encontrada.");

        // Check for unique constraints
        if (await _context.People.AnyAsync(p => p.NIF == person.NIF
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated NIF try detected.");
            throw new InvalidOperationException("O NIF da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(person.NISS)
            && await _context.People.AnyAsync(p => p.NISS == person.NISS
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated NISS try detected.");
            throw new InvalidOperationException("O NISS da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(person.IdentificationNumber)
            && await _context.People.AnyAsync(p => p.IdentificationNumber == person.IdentificationNumber
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated IdentificationNumber try detected.");
            throw new InvalidOperationException("O Número de Identificação da pessoa deve ser único. Já existe no sistema.");
        }

        if (!string.IsNullOrEmpty(person.Email)
            && await _context.People.AnyAsync(p => p.Email == person.Email
            && p.Id != existingPerson.Id))
        {
            _logger.LogWarning("Duplicated Email try detected.");
            throw new InvalidOperationException("O Email da pessoa deve ser único. Já existe no sistema.");
        }

        _context.Entry(existingPerson).CurrentValues.SetValues(Person.ConvertUpdateDtoToEntity(person));
        await _context.SaveChangesAsync();

        // Update cache
        var cacheKey = $"person:{existingPerson.Id}";
        await _cacheService.SetAsync(cacheKey, Person.ConvertEntityToRetrieveDto(existingPerson), TimeSpan.FromMinutes(30));
        await _cacheService.RemoveAsync("people:list");

        return Person.ConvertEntityToRetrieveDto(existingPerson);
    }
}