enum IdentificationType {
  ResidentialAuthorization = 'Autorização de Residência',
  CivilIdentification = 'Identificação Civil (CC/BI)',
  Military = 'Militar',
  Passport = 'Passaporte',
}

export const IDENTIFICATION_TYPES = Object.entries(IdentificationType).map(
  ([key, value]) => ({
    key,
    value,
  })
);
