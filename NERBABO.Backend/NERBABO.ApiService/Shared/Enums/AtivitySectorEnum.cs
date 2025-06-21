using System.ComponentModel;

namespace NERBABO.ApiService.Shared.Enums;

/// <summary>
/// This enumeration defines the different activity sectors available in the application.
/// It includes options such as "Unknown", "Agriculture", "Extractive Industries", "Manufacturing", and many others.
/// Use Humanizer to get the description of each enum value.
/// </summary>
public enum AtivitySectorEnum
{
    [Description("Não Especificado")]
    Unknown,

    [Description("Agricultura, produção animal, caça, floresta e pesca")]
    Agriculture,

    [Description("Indústrias extrativas")]
    ExtractiveIndustries,

    [Description("Indústrias transformadoras")]
    Manufacturing,

    [Description("Eletricidade, gás, vapor, água quente e fria e ar frio")]
    Utilities,

    [Description("Captação, tratamento e distribuição de água; saneamento, gestão de resíduos e despoluição")]
    WaterAndWaste,

    [Description("Construção")]
    Construction,

    [Description("Comércio por grosso e a retalho; reparação de veículos automóveis e motociclos")]
    TradeAndRepair,

    [Description("Transportes e armazenagem")]
    TransportAndStorage,

    [Description("Alojamento e restauração (restaurantes e similares)")]
    AccommodationAndFood,

    [Description("Atividades de informação e de comunicação")]
    ITAndCommunication,

    [Description("Atividades financeiras e de seguros")]
    FinanceAndInsurance,

    [Description("Atividades imobiliárias")]
    RealEstate,

    [Description("Atividades de consultoria, científicas, técnicas e similares")]
    ProfessionalServices,

    [Description("Atividades administrativas e dos serviços de apoio")]
    AdministrativeSupport,

    [Description("Administração Pública e Defesa; Segurança Social Obrigatória")]
    PublicAdministration,

    [Description("Educação")]
    Education,

    [Description("Atividades de saúde humana e apoio social")]
    HealthAndSocialWork,

    [Description("Atividades artísticas, de espetáculos, desportivas e recreativas")]
    ArtsAndEntertainment,

    [Description("Outras atividades de serviços")]
    OtherServices,

    [Description("Atividades das famílias empregadoras de pessoal doméstico e atividades de produção das famílias para uso próprio")]
    HouseholdActivities,

    [Description("Atividades dos organismos internacionais e outras instituições extraterritoriais")]
    InternationalOrganizations
}
