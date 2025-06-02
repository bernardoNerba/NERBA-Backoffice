using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

public enum IvaTypeEnum
{
    [Description("Não Especificado")]
    WithoutProof,
    [Description("Normal")]
    normal,
    [Description("Intermédia")]
    middle,
    [Description("Reduzida")]
    lower,
}
