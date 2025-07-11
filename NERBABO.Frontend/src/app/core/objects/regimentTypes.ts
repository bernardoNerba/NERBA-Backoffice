export enum RegimentTypeEnum {
  Online = 'Online',
  Presential = 'Presencial',
  Hybrid = 'Híbrido',
}

export const REGIMENTS = Object.entries(RegimentTypeEnum).map(
  ([key, value]) => ({
    key,
    value,
  })
);
