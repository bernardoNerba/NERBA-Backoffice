using System.ComponentModel.DataAnnotations;
using NERBABO.ApiService.Shared.Dtos;

namespace NERBABO.ApiService.Core.Enrollments.Dtos;

public class UpdateMTEnrollmentDto : EntityDto<long>
{
    [Required(ErrorMessage = "Relação Ação Módulo é um campo obrigatório.")]
    public long ModuleTeachingId { get; set; }

    [Required(ErrorMessage = "Formando é um campo obrigatório.")]
    public long StudentId { get; set; }

}
