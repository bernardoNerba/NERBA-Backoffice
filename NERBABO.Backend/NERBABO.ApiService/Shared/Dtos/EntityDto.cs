using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Shared.Dtos;

public class EntityDto
{
    [Required(ErrorMessage = "Id é um campo obrigatório.")]
    public long Id { get; set; } = 0;


}
