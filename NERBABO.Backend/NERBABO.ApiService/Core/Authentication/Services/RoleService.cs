using Microsoft.AspNetCore.Identity;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.Authentication.Dtos;
using NERBABO.ApiService.Shared.Models;
using NERBABO.ApiService.Shared.Services;

namespace NERBABO.ApiService.Core.Authentication.Services;

public class RoleService(
    UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<RoleService> logger,
        ICacheService cacheService
) : IRoleService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly ILogger<RoleService> _logger = logger;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<Result> UpdateUserRolesAsync(UserRoleDto userRole)
    {
        var userToModify = await _userManager.FindByIdAsync(userRole.UserId);
        if (userToModify is null)
        {
            _logger.LogError("Utilizador com ID {UserId} não encontrado.", userRole.UserId);
            return Result
                .Fail("Não encontrado.", "Utilizador não encontrado.",
                StatusCodes.Status404NotFound);
        }

        if (!userToModify.IsActive && userRole.Roles.Contains("Admin"))
            return Result
                .Fail("Erro de Validação", "Não é permitido atribuir o papél de Admin a um utilizador bloqueado.");

        // Check if the roles exist
        foreach (var role in userRole.Roles)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                _logger.LogError("O papel '{Role}' não existe no sistema.", role);
                return Result
                .Fail("Não encontrado.", $"O papel '{role}' não existe no sistema.",
                StatusCodes.Status404NotFound);
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
                return Result
                    .Fail("Erro de Validação", "Erro ao atribuir papeis.");

            }
        }

        // Add new roles
        if (rolesToAdd.Count != 0)
        {
            var addResult = await _userManager.AddToRolesAsync(userToModify, rolesToAdd);
            if (!addResult.Succeeded)
            {
                _logger.LogError("Erro ao adicionar papeis: {Errors}", string.Join(", ", addResult.Errors.Select(e => e.Description)));
                return Result.Fail("Erro de Validação", "Erro ao atribuir papeis.");
            }
        }

        await _cacheService.RemoveAsync("users:list");
        await _cacheService.RemoveAsync($"user:{userRole.UserId}");

        return Result
            .Ok("Papeis atribuídos com sucesso.", "Os papéis foram atribuídos com sucesso ao usuário.");

    }

}
