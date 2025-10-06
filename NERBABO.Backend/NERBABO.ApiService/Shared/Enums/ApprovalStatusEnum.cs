using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different approval status options for student action enrollments.
/// It includes options ranging from not specified to approved and rejected.
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum ApprovalStatusEnum
{
    [Description("Não Especificado")]
    NotSpecified,

    [Description("Aprovado")]
    Approved,

    [Description("Reprovado")]
    Rejected,

    [Description("Desistiu")]
    Dropped
}