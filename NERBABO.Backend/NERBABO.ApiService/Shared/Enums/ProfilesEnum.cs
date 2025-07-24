using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different types of profiles that a person can have within the application.
/// Includes the following profiles: 'Colaborator', 'Student' and 'Teacher'.
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum ProfilesEnum
{
    [Description("Colaborador")]
    Colaborator,
    [Description("Formando")]
    Student,
    [Description("Formador")]
    Teacher
}