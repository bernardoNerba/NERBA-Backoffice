using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Enrollments.Dtos;

public class CreateMTEnrollmentDto
{
    [Required(ErrorMessage = "Relação Ação Módulo é um campo obrigatório.")]
    public long ModuleTeachingId { get; set; }

    [Required(ErrorMessage = "Formando é um campo obrigatório.")]
    public long StudentId { get; set; }
}