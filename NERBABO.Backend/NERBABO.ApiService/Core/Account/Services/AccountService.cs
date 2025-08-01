using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Core.Account.Models;
using NERBABO.ApiService.Core.People.Cache;
using NERBABO.ApiService.Data;
using NERBABO.ApiService.Shared.Models;
using ZLinq;

namespace NERBABO.ApiService.Core.Account.Services;

public class AccountService(
    UserManager<User> userManager,
        AppDbContext context,
        ILogger<AccountService> logger,
        ICachePeopleRepository cachePeople
    ) : IAccountService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly AppDbContext _context = context;
    private readonly ILogger<AccountService> _logger = logger;
    private readonly ICachePeopleRepository _cachePeople = cachePeople;

    public async Task<Result<RetrieveUserDto>> CreateAsync(RegisterDto entityDto)
    {
        // Check email duplication
        if (await _userManager.FindByEmailAsync(entityDto.Email.ToLower()) is not null)
        {
            return Result<RetrieveUserDto>
                .Fail("Erro de Validação.", $"Email {entityDto.Email} já existe.");
        }

        // check username duplication
        if (await _userManager.FindByNameAsync(entityDto.UserName.ToLower()) is not null)
        {
            return Result<RetrieveUserDto>
                .Fail("Erro de Validação.", $"Nome de Utilizador {entityDto.UserName} já existe.");
        }

        // checks if the person exists
        var person = await _context.People.FindAsync(entityDto.PersonId);
        if (person is null)
            return Result<RetrieveUserDto>
                .Fail("Não encontrado.", $"A pessoa que tentou associar ao utilizador não existe.",
                StatusCodes.Status404NotFound);

        // checks if there is already a user associated with the person
        var user = await _context.Users.FirstOrDefaultAsync(x => x.PersonId == person.Id);
        if (user is not null)
            return Result<RetrieveUserDto>
                    .Fail("Erro de Validação", $"A pessoa que tentou associar já é um utilizador.");

        // Begin database transaction
        var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Create a new user object
            // TODO: Implement email confirmation logic
            var userToAdd = new User(entityDto.UserName.ToLower(), entityDto.Email.ToLower(), entityDto.PersonId);

            var result = await _userManager.CreateAsync(userToAdd, entityDto.Password);
            if (!result.Succeeded)
            {
                return Result<RetrieveUserDto>
                    .Fail("Erro de Validação.", "Formato da password Inálido",
                    result.Errors.Select(e => e.Description).ToList());
            }

            var roleAssignmentResult = await _userManager.AddToRoleAsync(userToAdd, "User");
            if (!roleAssignmentResult.Succeeded)
            {
                return Result<RetrieveUserDto>
                    .Fail("Falha ao atribuir a função", "Erro ao atribuir função ao utilizador.",
                    result.Errors.Select(e => e.Description).ToList());
            }

            await _cachePeople.RemovePeopleCacheAsync();

            return Result<RetrieveUserDto>
                .Ok(User.ConvertEntityToRetrieveDto(userToAdd, _userManager).Result,
                "Utilizador registado.", $"Utilizador {userToAdd.UserName} registado com sucesso.",
                StatusCodes.Status201Created);
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
    }

    public async Task<Result> BlockAsync(string userId)
    {
        var user = await _userManager.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return Result
                .Fail("Não encontrado.", "Utilizador não encontrado.", StatusCodes.Status404NotFound);

        // Check if the user is an admin and do not allow blocking
        if (await _userManager.IsInRoleAsync(user, "Admin"))
            return Result
                .Fail("Não permitido.", "Não é possível bloquear um utilizador com a função de administrador.",
                StatusCodes.Status403Forbidden);

        // Toggle the IsActive property
        user.IsActive = !user.IsActive;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result
                .Fail("Erro de Validação.", "Erro ao atualizar o utilizador.", errors);
        }

        var retrievedUserDto = await User.ConvertEntityToRetrieveDto(user, _userManager);

        // update cache
        await _cachePeople.RemovePeopleCacheAsync(user.PersonId);

        return Result
            .Ok("Estado da conta do utilizador alterado.", "Estado da conta do utilizador alterado com sucesso.");
    }

    public async Task<Result<IEnumerable<RetrieveUserDto>>> GetAllAsync()
    {

        // Fetch users from the database
        List<User> existingUsers = await _userManager.Users
            .Include(u => u.Person)
            .ToListAsync();

        // check if there is data
        if (existingUsers is null || existingUsers.Count == 0)
        {
            _logger.LogWarning("No users found in the database.");
            return Result<IEnumerable<RetrieveUserDto>>
                .Fail("Não encontrado.", "Não foram encontrados utilizadores.",
                StatusCodes.Status404NotFound);
        }

        // Query users
        var users = existingUsers
            .AsValueEnumerable()
            .Select(u => User.ConvertEntityToRetrieveDto(u, _userManager).Result)
            .OrderByDescending(u => u.Roles.Count)
            .ThenBy(u => u.FullName)
            .ToList();

        await _cachePeople.RemovePeopleCacheAsync();

        // Return the users sorted by roles quantity and then by full name
        return Result<IEnumerable<RetrieveUserDto>>
            .Ok(users);
    }

    public async Task<Result<RetrieveUserDto>> GetByIdAsync(string id)
    {

        // Get user from DB
        var userEntity = await _userManager.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (userEntity is null)
            return Result<RetrieveUserDto>
                .Fail("Não encontrado.", "Utilizador não encontrado.",
                StatusCodes.Status404NotFound);


        // Convert to DTO
        var user = await User.ConvertEntityToRetrieveDto(userEntity, _userManager);

        return Result<RetrieveUserDto>.Ok(user);
    }

    public async Task<Result<RetrieveUserDto>> UpdateAsync(UpdateUserDto model)
    {
        // Get the user from the database
        var user = await _userManager.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Id == model.Id);

        if (user is null)
            return Result<RetrieveUserDto>
                .Fail("Não encontrado", "Utilizador não encontrado.",
                StatusCodes.Status404NotFound);

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
                return Result<RetrieveUserDto>
                    .Fail("Password Inválida.", "O formato da password fornecida está inválido.");
        }

        // update the user
        var updateResult = await _userManager.UpdateAsync(user);

        // update cache
        await _cachePeople.RemovePeopleCacheAsync(user.PersonId);

        return Result<RetrieveUserDto>
            .Ok(User.ConvertEntityToRetrieveDto(user, _userManager).Result,
            "Utilizador atualizado.", $"Utilizador {user.UserName} atualizado.");
    }

    public async Task<Result> DeleteAsync(string id)
    {
        var existingUser = await _userManager.FindByIdAsync(id);
        if (existingUser is null)
            return Result
                .Fail("Não encontrado", "Utilizador não encontrado.",
                StatusCodes.Status404NotFound);
        
        
        await _userManager.DeleteAsync(existingUser);
        
        // update cache
        await _cachePeople.RemovePeopleCacheAsync(existingUser.PersonId);

        return Result
            .Ok("Utilizador eliminado.", $"Utilizador {existingUser.UserName} eliminado com sucesso.",
            StatusCodes.Status200OK);
    }
}
