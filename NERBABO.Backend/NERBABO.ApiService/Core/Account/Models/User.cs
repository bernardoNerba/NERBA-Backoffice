using Microsoft.AspNetCore.Identity;
using NERBABO.ApiService.Core.Account.Dtos;
using NERBABO.ApiService.Core.People.Models;

namespace NERBABO.ApiService.Core.Account.Models;

public class User : IdentityUser
{
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLogin { get; set; }


    public long PersonId { get; set; }
    public Person? Person { get; set; }

    public User() { }

    public User(string userName, string email, long personId) // Create user from account controller
    {
        UserName = userName;
        Email = email;
        EmailConfirmed = true;
        PersonId = personId;
        CreatedAt = DateTime.UtcNow;
        LastLogin = DateTime.UtcNow;
    }

    public async static Task<RetrieveUserDto> ConvertEntityToRetrieveDto(User user, UserManager<User> userManager)
    {
        if (user == null || user.Person == null)
            throw new KeyNotFoundException("Utilizador ou Pessoa Associado ao mesmo não encontrado.");

        var roles = await userManager.GetRolesAsync(user);

        return new RetrieveUserDto
        {
            Id = user.Id,
            PersonId = user.Person!.Id,
            FirstName = user.Person.FirstName,
            LastName = user.Person.LastName,
            PhoneNumber = user.PhoneNumber ?? "",
            Email = user.Email ?? "",
            UserName = user.UserName ?? "",
            Roles = [.. roles],
            IsActive = user.IsActive,
        };
    }
}
