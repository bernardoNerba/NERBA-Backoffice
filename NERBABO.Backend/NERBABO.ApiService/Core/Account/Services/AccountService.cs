using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Services;
using ZLinq;

namespace NERBABO.ApiService.Core.Account.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<AccountService> _logger;
    private readonly ICacheService _cacheService;
    public AccountService(
        UserManager<User> userManager,
        AppDbContext context,
        ILogger<AccountService> logger,
        ICacheService cacheService)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task RegistUserAsync(RegisterDto registerDto)
    {

        // Check email duplication
        if (await _userManager.FindByEmailAsync(registerDto.Email.ToLower()) != null)
        {
            _logger.LogWarning("Email {Email} already exists.", registerDto.Email);
            throw new InvalidOperationException($"Email {registerDto.Email} já existe.");
        }

        // check username duplication
        if (await _userManager.FindByNameAsync(registerDto.UserName.ToLower()) != null)
        {
            _logger.LogWarning("Username {UserName} already exists.", registerDto.UserName);
            throw new InvalidOperationException($"Nome de Utilizador {registerDto.UserName} já existe.");
        }

        // checks if the person exists
        if (!await _context.People.AnyAsync(p => p.Id == registerDto.PersonId))
        {
            _logger.LogWarning("Person with ID {PersonId} does not exist.", registerDto.PersonId);
            throw new InvalidOperationException($"A pessoa com o ID {registerDto.PersonId} não existe.");
        }

        // checks if there is already a user associated with the person
        if (_userManager.Users.Any(u => u.PersonId == registerDto.PersonId))
        {
            _logger.LogWarning("User with PersonId {PersonId} already exists.", registerDto.PersonId);
            throw new InvalidOperationException("Já existe um utilizador associado a esta pessoa.");
        }

        var transaction = await _context.Database.BeginTransactionAsync();

        try
        {

            // Create a new user object
            // TODO: Implement email confirmation logic
            var userToAdd = new User(registerDto.UserName.ToLower(), registerDto.Email.ToLower(), registerDto.PersonId);

            var result = await _userManager.CreateAsync(userToAdd, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("User creation failed for {UserName}. Errors: {Errors}",
                    registerDto.UserName, errors);
                throw new InvalidOperationException($"Falha ao criar o utilizador: {errors}");
            }

            var roleAssignmentResult = await _userManager.AddToRoleAsync(userToAdd, "User");
            if (!roleAssignmentResult.Succeeded)
            {
                var errors = string.Join(", ", roleAssignmentResult.Errors.Select(e => e.Description));
                _logger.LogWarning("Role assignment failed for {UserName}. Errors: {Errors}",
                    registerDto.UserName, errors);
                throw new InvalidOperationException($"Falha ao atribuir a função: {errors}");
            }


            await transaction.CommitAsync();
            await _cacheService.RemoveAsync("users:list");
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<RetrieveUserDto?> BlockUserAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);
            throw new Exception($"Utilizador com ID {userId} não encontrado.");
        }

        user.IsActive = !user.IsActive; // Toggle the IsActive property
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to block user with ID {UserId}. Errors: {Errors}", userId, errors);
            throw new Exception($"Falha ao bloquear o utilizador: {errors}");
        }

        var retrievedUserDto = await User.ConvertEntityToRetrieveDto(user, _userManager);

        await _cacheService.RemoveAsync("users:list");
        await _cacheService.SetAsync($"user:{user.Id}", retrievedUserDto, TimeSpan.FromMinutes(30));

        return retrievedUserDto;
    }

    public async Task<IEnumerable<RetrieveUserDto>> GetAllUsersAsync()
    {
        var cacheKey = "users:list";
        var usersToRetrieve = new List<RetrieveUserDto>();

        var cachedUsers = await _cacheService.GetAsync<IEnumerable<RetrieveUserDto>>(cacheKey);
        if (cachedUsers != null)
            return cachedUsers;

        // Get all users from the database
        List<User> users = await _userManager.Users
            .Include(u => u.Person)
            .ToListAsync();

        // Populate the list of retireve user DTOs
        foreach (var user in users)
        {
            var userDto = await User.ConvertEntityToRetrieveDto(user, _userManager);
            if (userDto != null)
            {
                usersToRetrieve.Add(userDto);
            }
        }

        await _cacheService.SetAsync(cacheKey, usersToRetrieve, TimeSpan.FromMinutes(30));

        // Return the users sorted by roles quantity and then by full name
        return usersToRetrieve
            .OrderByDescending(u => u.Roles.Count)
            .ThenBy(u => u.FullName);
    }

    public async Task<RetrieveUserDto?> GetUserByIdAsync(string id)
    {
        var cacheKey = $"user:{id}";

        var cachedUser = await _cacheService.GetAsync<RetrieveUserDto>(cacheKey);
        if (cachedUser != null)
            return cachedUser;

        // Get the user from the database
        var user = await _userManager.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) // No user?
        {
            _logger.LogWarning("User not found to perform query.");
            return null;
        }

        var retrievingUserDto = await User.ConvertEntityToRetrieveDto(user, _userManager);
        await _cacheService.SetAsync(cacheKey, retrievingUserDto, TimeSpan.FromMinutes(30));

        // Create the retrieve user DTO and return it
        return retrievingUserDto;
    }

    public async Task<bool> UpdateUserAsync(UpdateUserDto model)
    {
        // Get the user from the database
        var user = await _userManager.Users
            .Include(u => u.Person)
            .Where(u => u.Id == model.Id)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            _logger.LogWarning("User not found to perform query.");
            return false;
        }

        // assign the new values to the user
        user.Email = model.Email;
        user.UserName = model.UserName;


        if (!string.IsNullOrEmpty(model.NewPassword)) // must have password
        {
            // generate token to reset password and update it
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded) // failed to update password
            {
                _logger.LogWarning("Failed to update password.");
                return false;
            }
        }
        // update the user
        var updateResult = await _userManager.UpdateAsync(user);

        // update cache
        await _cacheService.RemoveAsync("users:list");
        await _cacheService.SetAsync($"user:{user.Id}", await User.ConvertEntityToRetrieveDto(user, _userManager), TimeSpan.FromMinutes(30));

        return updateResult.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User not found to perform query.");
            return false;
        }
        var result = await _userManager.DeleteAsync(user);

        // remove from cache
        await _cacheService.RemoveAsync("users:list");
        await _cacheService.RemoveAsync($"user:{user.Id}");

        return result.Succeeded;
    }

}
