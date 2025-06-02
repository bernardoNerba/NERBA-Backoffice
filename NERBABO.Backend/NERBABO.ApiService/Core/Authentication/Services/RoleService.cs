using Microsoft.AspNetCore.Identity;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Authentication.Dtos;

namespace NERBABO.ApiService.Core.Authentication.Services;

public class RoleService : IRoleService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<RoleService> _logger;
    public RoleService(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<RoleService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;

    }

    public async Task UpdateUserRolesAsync(UserRoleDto userRole)
    {
        var userToModify = await _userManager.FindByIdAsync(userRole.UserId);
        if (userToModify == null)
        {
            _logger.LogError("Utilizador com ID {UserId} não encontrado.", userRole.UserId);
            throw new Exception("Utilizador não encontrado.");
        }

        // Check if the roles exist
        foreach (var role in userRole.Roles)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                _logger.LogError("O papel '{Role}' não existe no sistema.", role);
                throw new Exception($"O papel '{role}' não existe no sistema.");
            }
        }

        var currentRoles = await _userManager.GetRolesAsync(userToModify);

        var rolesToAdd = userRole.Roles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(userRole.Roles).ToList();

        // Remove roles that are not in the new list
        if (rolesToRemove.Count != 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(userToModify, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                _logger.LogError("Erro ao remover papeis: {Errors}", string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                throw new Exception("Erro ao atribuir papeis.");
            }
        }

        // Add new roles
        if (rolesToAdd.Count != 0)
        {
            var addResult = await _userManager.AddToRolesAsync(userToModify, rolesToAdd);
            if (!addResult.Succeeded)
            {
                _logger.LogError("Erro ao adicionar papeis: {Errors}", string.Join(", ", addResult.Errors.Select(e => e.Description)));
                throw new Exception("Erro ao atribuir papeis.");
            }
        }

    }

}
