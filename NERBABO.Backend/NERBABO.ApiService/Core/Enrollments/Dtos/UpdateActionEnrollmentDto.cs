using System.ComponentModel.DataAnnotations;
using NERBABO.ApiService.Shared.Dtos;

namespace NERBABO.ApiService.Core.Enrollments.Dtos;

public class UpdateActionEnrollmentDto : EntityDto<long>
{
    [Required(ErrorMessage = "Ação é um campo obrigatório.")]
    public long ActionId { get; set; }

    [Required(ErrorMessage = "Formando é um campo obrigatório.")]
    public long StudentId { get; set; }
}