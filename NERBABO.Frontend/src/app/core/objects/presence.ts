export enum PresenceEnum {
  Unknown = 'Não Especificado',
  Present = 'Presente',
  Absent = 'Faltou',
}

export const PRESENCES = Object.entries(PresenceEnum).map(([key, value]) => ({
  key,
  value,
}));