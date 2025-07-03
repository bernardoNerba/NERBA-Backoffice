export enum ActivitySector {
  Unknown = 'Não Especificado',
  Agriculture = 'Agricultura, produção animal, caça, floresta e pesca',
  ExtractiveIndustries = 'Indústrias extrativas',
  Manufacturing = 'Indústrias transformadoras',
  Utilities = 'Eletricidade, gás, vapor, água quente e fria e ar frio',
  WaterAndWaste = 'Captação, tratamento e distribuição de água; saneamento, gestão de resíduos e despoluição',
  Construction = 'Construção',
  TradeAndRepair = 'Comércio por grosso e a retalho; reparação de veículos automóveis e motociclos',
  TransportAndStorage = 'Transportes e armazenagem',
  AccommodationAndFood = 'Alojamento e restauração (restaurantes e similares)',
  ITAndCommunication = 'Atividades de informação e de comunicação',
  FinanceAndInsurance = 'Atividades financeiras e de seguros',
  RealEstate = 'Atividades imobiliárias',
  ProfessionalServices = 'Atividades de consultoria, científicas, técnicas e similares',
  AdministrativeSupport = 'Atividades administrativas e dos serviços de apoio',
  PublicAdministration = 'Administração Pública e Defesa; Segurança Social Obrigatória',
  Education = 'Educação',
  HealthAndSocialWork = 'Atividades de saúde humana e apoio social',
  ArtsAndEntertainment = 'Atividades artísticas, de espetáculos, desportivas e recreativas',
  OtherServices = 'Outras atividades de serviços',
  HouseholdActivities = 'Atividades das famílias empregadoras de pessoal doméstico e atividades de produção das famílias para uso próprio',
  InternationalOrganizations = 'Atividades dos organismos internacionais e outras instituições extraterritoriais',
}

export const ACTIVITY_SECTOR = Object.entries(ActivitySector).map(
  ([key, value]) => ({
    key,
    value,
  })
);
