using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Exceptions;
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
            throw new ValidationException($"Email {registerDto.Email} já existe.");
        }

        // check username duplication
        if (await _userManager.FindByNameAsync(registerDto.UserName.ToLower()) != null)
        {
            throw new ValidationException($"Nome de Utilizador {registerDto.UserName} já existe.");
        }

        // checks if the person exists
        if (!await _context.People.AnyAsync(p => p.Id == registerDto.PersonId))
        {
            throw new ValidationException($"A pessoa com o ID {registerDto.PersonId} não existe.");
        }

        // checks if there is already a user associated with the person
        if (_userManager.Users.Any(u => u.PersonId == registerDto.PersonId))
        {
            throw new ValidationException("Já existe um utilizador associado a esta pessoa.");
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
                throw new InvalidOperationException($"{errors}");
            }

            var roleAssignmentResult = await _userManager.AddToRoleAsync(userToAdd, "User");
            if (!roleAssignmentResult.Succeeded)
            {
                var errors = string.Join(", ", roleAssignmentResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Falha ao atribuir a função: {errors}");
            }

            // Update the cache
            await transaction.CommitAsync();
            await _cacheService.RemoveAsync("users:list");
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task BlockUserAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new KeyNotFoundException($"Utilizador não encontrado.");

        // Toggle the IsActive property
        user.IsActive = !user.IsActive;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ValidationException($"Falha ao bloquear o utilizador: {errors}");
        }

        var retrievedUserDto = await User.ConvertEntityToRetrieveDto(user, _userManager);

        await _cacheService.RemoveAsync("users:list");
        await _cacheService.SetAsync($"user:{user.Id}", retrievedUserDto, TimeSpan.FromMinutes(30));
    }

    public async Task<IEnumerable<RetrieveUserDto>> GetAllUsersAsync()
    {

        // Check if the users are cached
        var cacheKey = "users:list";
        var cachedUsers = await _cacheService.GetAsync<IEnumerable<RetrieveUserDto>>(cacheKey);
        if (cachedUsers != null)
            return cachedUsers;

        // Fetch users from the database
        List<User> existingUsers = await _userManager.Users
            .Include(u => u.Person)
            .ToListAsync() ?? throw new("No users found in the database.");

        if (existingUsers == null || existingUsers.Count == 0)
        {
            _logger.LogWarning("No users found in the database.");
            throw new KeyNotFoundException("No users found in the database.");
        }

        // Get all users from the database
        var users = existingUsers
            .AsValueEnumerable()
            .Select(u => User.ConvertEntityToRetrieveDto(u, _userManager).Result)
            .OrderByDescending(u => u?.Roles.Count)
            .ThenBy(u => u?.FullName)
            .ToList();

        await _cacheService.SetAsync(cacheKey, users, TimeSpan.FromMinutes(30));

        // Return the users sorted by roles quantity and then by full name
        return users!;
    }

    public async Task<RetrieveUserDto> GetUserByIdAsync(string id)
    {

        // Try to get from cache
        var cacheKey = $"user:{id}";
        var cachedUser = await _cacheService.GetAsync<RetrieveUserDto>(cacheKey);
        if (cachedUser != null)
            return cachedUser;

        // Get user from DB
        var userEntity = await _userManager.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new KeyNotFoundException("Utilizador não encontrado."); ;


        // Convert to DTO
        var userDto = await User.ConvertEntityToRetrieveDto(userEntity, _userManager);

        // Cache the DTO
        await _cacheService.SetAsync(cacheKey, userDto, TimeSpan.FromMinutes(30));

        return userDto;
    }

    public async Task UpdateUserAsync(UpdateUserDto model)
    {
        // Get the user from the database
        var user = await _userManager.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == model.Id)
            ?? throw new KeyNotFoundException("Utilizador não encontrado");

        // assign the new values to the user
        user.Email = model.Email;
        user.UserName = model.UserName;

        // must have password
        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            // generate token to reset password and update it
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded) 
                throw new ValidationException("Formato de password inválido");
        }

        // update the user
        var updateResult = await _userManager.UpdateAsync(user);

        // update cache
        await _cacheService.RemoveAsync("users:list");
        await _cacheService.SetAsync($"user:{user.Id}", await User.ConvertEntityToRetrieveDto(user, _userManager), TimeSpan.FromMinutes(30));
    }
}
