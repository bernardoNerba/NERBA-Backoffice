export enum DestinatorTypeEnum {
  Unkown = 'Não Especificado',
  ByOtherEmployeed = 'Empregado Por Conta De Outrem',
  SelfEmployeed = 'Empregado Por Conta Própria',
  FirstJobUnemployeed = 'Desempregado Primeiro Emprego',
  LongTimeUnemployeed = 'DLD',
  NotLongTimeUnemployeed = 'NDLD',
  Inactive = 'Inátivo',
}
// Empregado Por Conta De Outrem
export const DESTINATORS = Object.entries(DestinatorTypeEnum).map(
  ([key, value]) => ({
    key,
    value,
  })
);
