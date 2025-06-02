using System;
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

        _context.People.Add(newPerson);
        await _context.SaveChangesAsync();
        return Person.ConvertEntityToRetrieveDto(newPerson);
    }

    public Task<bool> DeletePersonAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<RetrievePersonDto>> GetAllPeopleAsync()
    {
        var cacheKey = "people";
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

    public Task<RetrievePersonDto?> GetPersonByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<RetrievePersonDto?> UpdatePersonAsync(UpdatePersonDto person)
    {
        throw new NotImplementedException();
    }
}