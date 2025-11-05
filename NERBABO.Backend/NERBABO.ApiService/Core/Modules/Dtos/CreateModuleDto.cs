using NerbaApp.Api.Validators;
using NERBABO.ApiService.Helper.Validators;
using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Modules.Dtos
{
    public class CreateModuleDto
    {
        [Required(ErrorMessage = "Nome do Módulo é um campo obrigatório.")]
        [ValidateLengthIfNotEmpty(255, MinimumLength = 3,
            ErrorMessage = "O Nome do Módulo deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
        public string Name { get; set; } = string.Empty;

        [ValidateHours(0, 1000, true)]
        public float Hours { get; set; }
    }
}
