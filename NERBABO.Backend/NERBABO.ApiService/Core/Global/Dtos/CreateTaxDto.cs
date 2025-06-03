using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Global.Dtos;


public class CreateTaxDto
{
    [Required(ErrorMessage = "Regime é um campo obrigatório")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Regime deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valor percentual é um campo obrigatório")]
    [Range(0, 100, ErrorMessage = "Valor percentual deve estar entre {1} e {2}")]
    public int ValuePercent { get; set; }

    [Required(ErrorMessage = "Tipo é um campo obrigatório")]
    public string Type { get; set; } = string.Empty;

}

