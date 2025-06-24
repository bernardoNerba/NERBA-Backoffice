using NerbaApp.Api.Validators;
using NERBABO.ApiService.Helper.Validators;
using NERBABO.ApiService.Shared.Dtos;
using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Courses.Dtos
{
    public class UpdateCourseDto: EntityDto<long>
    {
        [Required(ErrorMessage = "Enquadramento é um campo obrigatório.")]
        public long FrameId { get; set; }

        [Required(ErrorMessage = "Título / Nome do curso é um campo obrigatório.")]
        [ValidateLengthIfNotEmpty(255, MinimumLength = 3,
        ErrorMessage = "Título / Nome do curso deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
        public string Title { get; set; } = string.Empty;

        [ValidateLengthIfNotEmpty(510, MinimumLength = 3,
        ErrorMessage = "Objetivos do curso deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
        public string? Objectives { get; set; } = string.Empty;

        public List<string>? Destinators { get; set; } = [];

        [ValidateLengthIfNotEmpty(55, MinimumLength = 3,
        ErrorMessage = "Área do curso deve conter pelo menos {2} caracteres e um máximo de {1} caracteres")]
        public string? Area { get; set; } = string.Empty;

        [ValidateHours(0, 1000, true)]
        public float TotalDuration { get; set; }
        public bool Status { get; set; }
        public string? MinHabilitationLevel { get; set; } = string.Empty;
    }
}
