using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different levels of status.
/// It includes options ranging from not started to completed.
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum StatusEnum
{
    [Description("Não Iniciado")]
    NotStarted,

    [Description("Em Progresso")]
    InProgress,

    [Description("Concluído")]
    Completed,

    [Description("Cancelado")]
    Cancelled
}
