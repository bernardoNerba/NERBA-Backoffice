using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different types of identification that can be used within the application.
/// It includes options such as "Unknown", "Residential Authorization", "Civil Identification", "Military", and "Passport".
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum IdentificationTypeEnum
{
    [Description("Não Especificado")]
    Unknown,

    [Description("Autorização de Residência")]
    ResidentialAuthorization,

    [Description("Identificação Civil (CC/BI)")]
    CivilIdentification,

    [Description("Militar")]
    Military,

    [Description("Passaporte")]
    Passport
}
