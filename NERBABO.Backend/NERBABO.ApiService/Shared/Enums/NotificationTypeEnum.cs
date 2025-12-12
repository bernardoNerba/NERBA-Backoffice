using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different types of notifications that can be generated in the system.
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum NotificationTypeEnum
{
    [Description("Documento em Falta")]
    MissingDocument = 1,

    [Description("Informação Incompleta")]
    IncompleteInformation = 2,

    [Description("Alerta Geral")]
    GeneralAlert = 3
}
