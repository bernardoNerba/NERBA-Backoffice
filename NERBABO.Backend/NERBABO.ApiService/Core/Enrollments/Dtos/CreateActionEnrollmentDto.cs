using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.Enrollments.Dtos;

public class CreateActionEnrollmentDto
{
    [Required(ErrorMessage = "Ação é um campo obrigatório.")]
    public long ActionId { get; set; }

    [Required(ErrorMessage = "Formando é um campo obrigatório.")]
    public long StudentId { get; set; }
}