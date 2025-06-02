using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

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
