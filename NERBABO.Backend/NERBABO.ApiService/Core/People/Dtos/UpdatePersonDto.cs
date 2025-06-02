using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Humanizer;
using NerbaApp.Api.Validators;
using NERBABO.ApiService.Helper.Validators;
using NERBABO.ApiService.Shared.Dtos;
using NERBABO.ApiService.Shared.Enums;

namespace NERBABO.ApiService.Core.People.Dtos;

public class UpdatePersonDto : EntityDto
{
    [Required(ErrorMessage = "Primeiro Nome é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(100, MinimumLength = 3,
    ErrorMessage = "Primeiro Nome deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Último Nome é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(100, MinimumLength = 3,
        ErrorMessage = "Ultimo Nome deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "NIF é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(9, MinimumLength = 9, ErrorMessage = "NIF deve conter exatemente 9 caractéres.")]
    [AllNumbers(ErrorMessage = "NIF todos os caractéres devem ser números")]
    public string NIF { get; set; } = string.Empty;

    [ValidateLengthIfNotEmpty(10, MinimumLength = 5, ErrorMessage = "Numero de Identificação deve conter até 10 caractéres.")]
    public string? IdentificationNumber { get; set; } = string.Empty;

    [AllowNull]
    [FutureDate(ErrorMessage = "Data de Validação da Identificação expirou.")]
    public string? IdentificationValidationDate { get; set; } = null;

    [ValidateLengthIfNotEmpty(11, MinimumLength = 11, ErrorMessage = "NISS deve conter exatemente 11 caractéres.")]
    [AllNumbers(ErrorMessage = "NISS todos os caractéres devem ser números")]
    public string? NISS { get; set; } = string.Empty;

    [ValidateLengthIfNotEmpty(25, MinimumLength = 25, ErrorMessage = "IBAN deve conter exatemente 25 caractéres.")]
    public string? IBAN { get; set; } = string.Empty;

    [AllowNull]
    [PastDate(ErrorMessage = "A data de nascimento deve ser do passado.")]
    public string? BirthDate { get; set; } = null;

    public string? Address { get; set; } = string.Empty;

    [ZipCode]
    public string? ZipCode { get; set; } = string.Empty;

    [ValidateLengthIfNotEmpty(9, MinimumLength = 9, ErrorMessage = "Numero de Telefóne deve conter extamente 9 caractéres.")]
    [AllNumbers(ErrorMessage = "Número de Telefóne todos os caractéres devem ser números.")]
    public string? PhoneNumber { get; set; } = string.Empty;

    [RegularExpression(@"^(?:[^@\s]+@[^@\s]+\.[^@\s]+)?$", ErrorMessage = "Email tem formato inválido.")]
    public string? Email { get; set; }

    [ValidateLengthIfNotEmpty(100, MinimumLength = 3,
        ErrorMessage = "Naturalidade deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string? Naturality { get; set; } = string.Empty;

    [ValidateLengthIfNotEmpty(100, MinimumLength = 3,
        ErrorMessage = "Nacionalidade deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string? Nationality { get; set; } = string.Empty;

    public string? Gender { get; set; } = GenderEnum.Unknown.Humanize().Transform(To.TitleCase);
    public string? Habilitation { get; set; } = HabilitationEnum.WithoutProof.Humanize().Transform(To.TitleCase);
    public string? IdentificationType { get; set; } = IdentificationTypeEnum.Unknown.Humanize().Transform(To.TitleCase);

}
