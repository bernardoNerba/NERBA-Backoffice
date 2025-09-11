using System.ComponentModel.DataAnnotations;
using NERBABO.ApiService.Shared.Dtos;

namespace NERBABO.ApiService.Core.ModuleAvaliations.Dtos;

public class UpdateModuleAvaliationDto : EntityDto<long>
{
    [Required(ErrorMessage = "Grade é obrigatório")]
    [Range(0, 5, ErrorMessage = "Grade deve estar entre 0 e 5")]
    public int Grade { get; set; }
}
