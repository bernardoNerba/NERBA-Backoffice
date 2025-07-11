using System.ComponentModel.DataAnnotations;
using NerbaApp.Api.Validators;
using NERBABO.ApiService.Helper.Validators;

namespace NERBABO.ApiService.Core.Global.Dtos;

public class UpdateGeneralInfoDto
{
    [Required(ErrorMessage = "Designação é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(255, MinimumLength = 3,
    ErrorMessage = "Designação deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string Designation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Taxa Iva é um campo obrigatório")]
    public int IvaId { get; set; }

    [Required(ErrorMessage = "Sede é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(500, MinimumLength = 3,
        ErrorMessage = "Sede deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string Site { get; set; } = string.Empty;

    [Required(ErrorMessage = "Valor Hora do Formador é um campo obrigatório")]
    [Range(0, float.MaxValue, ErrorMessage = "Valor Hora do Formado, introduza um valor que possa ser traduzido valor monetário")]
    public float HourValueTeacher { get; set; }

    [Required(ErrorMessage = "Valor Hora Alimentação é um campo obrigatório")]
    [Range(0, float.MaxValue, ErrorMessage = "Valor Hora Alimentação, introduza um valor que possa ser traduzido valor monetário")]
    public float HourValueAlimentation { get; set; }

    [Required(ErrorMessage = "Entidade Bancária é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(50, MinimumLength = 3,
        ErrorMessage = "Entidade Bancária deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string BankEntity { get; set; } = string.Empty;

    [Required(ErrorMessage = "IBAN é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(25, MinimumLength = 25,
        ErrorMessage = "IBAN deve conter exatamente {1} caracteres")]
    public string Iban { get; set; } = string.Empty;

    [Required(ErrorMessage = "NIPC é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(9, MinimumLength = 9,
        ErrorMessage = "NIPC deve conter exatamente {1} caracteres")]
    [AllNumbers(ErrorMessage = "NIPC deve conter somente números.")]
    public string Nipc { get; set; } = string.Empty;
    public string LogoFinancing { get; set; } = string.Empty;

}
