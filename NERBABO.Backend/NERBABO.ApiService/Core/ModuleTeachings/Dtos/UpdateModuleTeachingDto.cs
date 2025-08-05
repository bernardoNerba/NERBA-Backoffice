using System;
using System.ComponentModel.DataAnnotations;
using NERBABO.ApiService.Shared.Dtos;

namespace NERBABO.ApiService.Core.ModuleTeachings.Dtos;

public class UpdateModuleTeachingDto : EntityDto<long>
{
    [Required(ErrorMessage = "Formador é um campo obrigatório.")]
    public long TeacherId { get; set; }
    
    [Required(ErrorMessage = "Ação Formação é um campo obrigatório.")]
    public long ActionId { get; set; }

    [Required(ErrorMessage = "Módulo é um campo obrigatório.")]
    public long ModuleId { get; set; }

}
