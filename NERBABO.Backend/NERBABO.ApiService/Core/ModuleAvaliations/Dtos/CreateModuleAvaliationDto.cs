using System.ComponentModel.DataAnnotations;

namespace NERBABO.ApiService.Core.ModuleAvaliations.Dtos;

public class CreateModuleAvaliationDto
{
    [Required(ErrorMessage = "ModuleTeachingId é obrigatório")]
    public long ModuleTeachingId { get; set; }
    
    [Required(ErrorMessage = "ActionEnrollmentId é obrigatório")]
    public long ActionEnrollmentId { get; set; }
    
    [Required(ErrorMessage = "Grade é obrigatório")]
    [Range(0, 5, ErrorMessage = "Grade deve estar entre 0 e 5")]
    public int Grade { get; set; }
}
