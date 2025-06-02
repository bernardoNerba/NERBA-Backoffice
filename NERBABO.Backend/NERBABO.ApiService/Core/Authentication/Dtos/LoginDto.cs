using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Authentication.Dtos;

public class LoginDto
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public LoginDto() { }

    public LoginDto(string cred, string pass)
    {
        UsernameOrEmail = cred;
        Password = pass;
    }
}
