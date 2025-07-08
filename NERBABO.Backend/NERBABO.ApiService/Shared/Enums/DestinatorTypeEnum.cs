using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different types of destinators that a course is ment for.
/// Empregado por conta de outrem
/// Empregado por conta própria
/// Desempregado Primeiro Emprego
/// DLD - Desempregado de Longa Duração
/// NDLD
/// Inativo
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum DestinatorTypeEnum
{
    [Description("Não Especificado")]
    Unkown,
    
    [Description("Empregado por conta de outrem")]
    ByOtherEmployeed,

    [Description("Empregado por conta própria")]
    SelfEmployeed,

    [Description("Desempregado Primeiro Emprego")]
    FirstJobUnemployeed,

    [Description("DLD")]
    LongTimeUnemployeed,

    [Description("NDLD")]
    NotLongTimeUnemployeed,

    [Description("Inátivo")]
    Inactive
}
