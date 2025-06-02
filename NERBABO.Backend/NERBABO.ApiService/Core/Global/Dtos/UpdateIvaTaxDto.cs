using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Global.Dtos;

public class UpdateIvaTaxDto
{
    [Required(ErrorMessage = "Id é um campo obrigatório")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Regime IVA é um campo obrigatório")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Regime IVA deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valor percentual IVA é um campo obrigatório")]
    [Range(0, 100, ErrorMessage = "Valor percentual IVA deve estar entre {1} e {2}")]
    public int ValuePercent { get; set; }

    public bool IsActive { get; set; }

}
