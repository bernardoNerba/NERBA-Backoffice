export enum DestinatorTypeEnum {
  Unkown = 'Não Especificado',
  ByOtherEmployeed = 'Empregado por conta de outrem',
  SelfEmployeed = 'Empregado por conta própria',
  FirstJobUnemployeed = 'Desempregado Primeiro Emprego',
  LongTimeUnemployeed = 'DLD',
  NotLongTimeUnemployeed = 'NDLD',
  Inactive = 'Inátivo',
}

export const DESTINATORS = Object.entries(DestinatorTypeEnum).map(
  ([key, value]) => ({
    key,
    value,
  })
);
