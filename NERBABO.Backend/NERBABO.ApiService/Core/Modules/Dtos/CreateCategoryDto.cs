using System.ComponentModel.DataAnnotations;
using NerbaApp.Api.Validators;

namespace NERBABO.ApiService.Core.Modules.Dtos;

public struct CreateCategoryDto
{
    [Required(ErrorMessage = "Nome da Categoria de Módulo é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(155, MinimumLength = 3,
        ErrorMessage = "O Nome da Categoria de Módulo deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public required string Name { get; set; }
    
    [Required(ErrorMessage = "Abreviatura da Categoria de Módulo é um campo obrigatório.")]
    [ValidateLengthIfNotEmpty(15, MinimumLength = 1,
        ErrorMessage = "O Abreviatura da Categoria de Módulo deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
    public required string ShortenName { get; set; }
}