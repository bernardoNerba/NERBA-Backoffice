using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

public enum GenderEnum
{
    [Description("Não Especificado")]
    Unknown,

    [Description("Masculino")]
    M,

    [Description("Feminino")]
    F,

    [Description("Outro")]
    Other
}
