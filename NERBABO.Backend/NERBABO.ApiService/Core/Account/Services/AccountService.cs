using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Data;

namespace NERBABO.ApiService.Core.Account.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<AccountService> _logger;
    public AccountService(
        UserManager<User> userManager,
        AppDbContext context,
        ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task RegistUserAsync(RegisterDto registerDto)
    {
        // Check email duplication
        if (await _userManager.FindByEmailAsync(registerDto.Email.ToLower()) != null)
        {
            _logger.LogWarning("Email {Email} already exists.", registerDto.Email);
            throw new Exception($"Email {registerDto.Email} já existe.");
        }

        // check username duplication
        if (await _userManager.FindByNameAsync(registerDto.UserName.ToLower()) != null)
        {
            _logger.LogWarning("Username {UserName} already exists.", registerDto.UserName);
            throw new Exception($"Nome de Utilizador {registerDto.UserName} já existe.");
        }

        // checks if the person exists
        if (!await _context.People.AnyAsync(p => p.Id == registerDto.PersonId))
        {
            _logger.LogWarning("Person with ID {PersonId} does not exist.", registerDto.PersonId);
            throw new Exception($"A pessoa com o ID {registerDto.PersonId} não existe.");
        }

        // checks if there is already a user associated with the person
        if (_userManager.Users.Any(u => u.PersonId == registerDto.PersonId))
        {
            _logger.LogWarning("User with PersonId {PersonId} already exists.", registerDto.PersonId);
            throw new Exception("Já existe um utilizador associado a esta pessoa.");
        }

        // Create a new user object
        // TODO: Implement email confirmation logic
        var userToAdd = new User(registerDto.UserName.ToLower(), registerDto.Email.ToLower(), registerDto.PersonId);

        var result = await _userManager.CreateAsync(userToAdd, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("User creation failed for {UserName}. Errors: {Errors}",
                registerDto.UserName, errors);
            throw new Exception($"Falha ao criar o utilizador: {errors}");
        }

        var roleAssignmentResult = await _userManager.AddToRoleAsync(userToAdd, "User");
        if (!roleAssignmentResult.Succeeded)
        {
            var errors = string.Join(", ", roleAssignmentResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Role assignment failed for {UserName}. Errors: {Errors}",
                registerDto.UserName, errors);
            throw new Exception($"Falha ao atribuir a função: {errors}");
        }


    }

    public async Task<User> BlockUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
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

        return user;
    }
}
