using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different possibilities for students and teachers presences
/// in sessions.
/// It includes the following options: Unknown, Present, Absent.
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum PresenceEnum
{
    [Description("Não Especificado")]
    Unknown,

    [Description("Presente")]
    Present,

    [Description("Faltou")]
    Absent,
}
