using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different Gender options available in the application.
/// It includes options such as Unkown, M (Male), F (Female), and Other.
/// Use Humanizer to get the description of each enum value.
/// </summary>
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
