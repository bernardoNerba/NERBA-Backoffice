using System;
using System.ComponentModel.DataAnnotations;
using NerbaApp.Api.Validators;

namespace NERBABO.ApiService.Core.Teachers.Dtos;

public class CreateTeacherDto
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

    [ValidateLengthIfNotEmpty(55, MinimumLength = 3,
        ErrorMessage = "Competências deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public string Competences { get; set; } = string.Empty;
}
