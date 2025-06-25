using NerbaApp.Api.Validators;
using NERBABO.ApiService.Helper.Validators;
using NERBABO.ApiService.Shared.Dtos;
using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Actions.Dtos
{
    public class CreateCourseActionDto
    {
        [Required(ErrorMessage = "Curso é um campo obrigatório.")]
        public long CourseId { get; set; }

        [Required(ErrorMessage = "Coordenador é um campo obrigatório.")]
        [ValidateLengthIfNotEmpty(255, MinimumLength = 3,
        ErrorMessage = "Título deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Código Administrativo é um campo obrigatório.")]
        [ValidateLengthIfNotEmpty(5, MinimumLength = 10, ErrorMessage = "NIF deve conter entre {1} a {2} números.")]
        [AllNumbers(ErrorMessage = "Código Administrativo todos os caractéres devem ser números")]
        public string AdministrationCode { get; set; } = string.Empty;
        public string? Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Localidade é um campo obrigatório.")]
        public string Locality { get; set; } = string.Empty;
        public List<string> WeekDays { get; set; } = [];

        [Required(ErrorMessage = "Data de Início é um campo obrigatório.")]
        [FutureDate(ErrorMessage = "A data de ínicio deve ser uma data futura.")]
        public string StartDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data de Fim é um campo obrigatório.")]
        [FutureDate(ErrorMessage = "A data de fim deve ser uma data futura.")]
        public string EndDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Estado é um campo obrigatório.")]
        public string Regiment { get; set; } = string.Empty;
    }
}
