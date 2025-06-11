using System.ComponentModel.DataAnnotations;
using NerbaApp.Api.Validators;
using NERBABO.ApiService.Shared.Dtos;

namespace NERBABO.ApiService.Core.Teachers.Dtos;

public class UpdateTeacherDto : EntityDto
{
    [Required(ErrorMessage = "Regime IVA é um campo obrigatório.")]
    public int IvaRegimeId { get; set; }

    [Required(ErrorMessage = "Regime IRS é um campo obrigatório.")]
    public int IrsRegimeId { get; set; }

    [Required(ErrorMessage = "É obrigatório associar uma pessoa ao utilizador.")]
    public long PersonId { get; set; }

    [Required(ErrorMessage = "CCP é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(55, MinimumLength = 3,
        ErrorMessage = "CCP deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string Ccp { get; set; } = string.Empty;

    [Required(ErrorMessage = "Competências é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(55, MinimumLength = 3,
        ErrorMessage = "Competências deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string Competences { get; set; } = string.Empty;

    public bool IsLecturingFM { get; set; }
    public bool IsLecturingCQ { get; set; }

}
