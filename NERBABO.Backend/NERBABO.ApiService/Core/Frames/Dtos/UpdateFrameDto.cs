using System.ComponentModel.DataAnnotations;
using NerbaApp.Api.Validators;
using NERBABO.ApiService.Shared.Dtos;

namespace NERBABO.ApiService.Core.Frames.Dtos;

public class UpdateFrameDto : EntityDto<long>
{
    [Required(ErrorMessage = "Programa é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(150, MinimumLength = 3,
    ErrorMessage = "O Programa deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string Program { get; set; } = string.Empty;

    [Required(ErrorMessage = "Intervenção é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(55, MinimumLength = 3,
        ErrorMessage = "O Intervenção deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string Intervention { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tipo de Interveção é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(150, MinimumLength = 3,
        ErrorMessage = "O Tipo de Interveção deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string InterventionType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Operação é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(150, MinimumLength = 3,
        ErrorMessage = "O Operação deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string Operation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Operação Tipo é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(150, MinimumLength = 3,
        ErrorMessage = "O Operação Tipo deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string OperationType { get; set; } = string.Empty;

    public IFormFile? ProgramLogoFile { get; set; }

    public IFormFile? FinancementLogoFile { get; set; }

    public bool RemoveProgramLogo { get; set; } = false;

    // Note: RemoveFinancementLogo should not be allowed since FinancementLogo is required
    // This property is kept for API consistency but will be validated in the service layer
    public bool RemoveFinancementLogo { get; set; } = false;

}
