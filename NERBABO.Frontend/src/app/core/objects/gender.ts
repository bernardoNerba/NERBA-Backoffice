export enum GenderEnum {
  Unkown = 'Não Especificado',
  M = 'Masculino',
  F = 'Feminino',
  Other = 'Outro',
}
export const GENDERS = Object.entries(GenderEnum).map(([key, value]) => ({
  key,
  value,
}));
