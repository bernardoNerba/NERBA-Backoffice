using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Authentication.Dtos;

public class UserRoleDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Pelo menos 1 papel é obrigatório.")]
    public List<string> Roles { get; set; } = [];

    [Required]
    public string UserId { get; set; } = string.Empty;

}
