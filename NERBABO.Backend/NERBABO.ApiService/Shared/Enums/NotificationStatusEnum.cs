using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different statuses a notification can have.
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum NotificationStatusEnum
{
    [Description("Não Lida")]
    Unread = 1,

    [Description("Lida")]
    Read = 2,

    [Description("Arquivada")]
    Archived = 3
}
