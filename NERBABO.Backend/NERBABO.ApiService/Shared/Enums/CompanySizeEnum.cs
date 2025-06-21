using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different company sizes available in the application.
/// It includes options such as Micro, Small, and Medium.
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum CompanySizeEnum
{
    [Description("Micro")]
    Micro,

    [Description("Pequena")]
    Small,

    [Description("Média")]
    Medium
}

