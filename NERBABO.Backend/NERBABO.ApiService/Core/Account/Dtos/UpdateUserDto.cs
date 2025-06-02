using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Account.Dtos;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Id é um campo obrigatório")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é um campo obrigatório")]
    [EmailAddress(ErrorMessage = "Formato do email inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username é um campo obrigatório.")]
    [StringLength(30, MinimumLength = 3,
        ErrorMessage = "Nome de Utilizador deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password é um campo obrigatório")]
    [StringLength(30, MinimumLength = 8,
                ErrorMessage = "A password deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string NewPassword { get; set; } = string.Empty;
}
