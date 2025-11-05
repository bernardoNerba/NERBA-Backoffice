using System.ComponentModel.DataAnnotations;
using NerbaApp.Api.Validators;

namespace NERBABO.ApiService.Core.Modules.Dtos;

public struct UpdateCategoryDto
{
    [Required(ErrorMessage = "Id é um campo obrigatório.")]
    public required long Id { get; set; }

    [Required(ErrorMessage = "Nome da Categoria de Módulo é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(155, MinimumLength = 3,
        ErrorMessage = "O Nome da Categoria de Módulo deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public required string Name { get; set; }
    
    [Required(ErrorMessage = "Abreviatura da Categoria de Módulo é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(155, MinimumLength = 3,
        ErrorMessage = "O Abreviatura da Categoria de Módulo deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public required string ShortenName { get; set; }
}