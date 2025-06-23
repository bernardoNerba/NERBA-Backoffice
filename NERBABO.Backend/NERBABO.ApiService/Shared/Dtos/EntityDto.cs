using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Shared.Dtos;

public class EntityDto<T>
{
    [Required(ErrorMessage = "Id é um campo obrigatório.")]
    public required T Id { get; set; }
}
