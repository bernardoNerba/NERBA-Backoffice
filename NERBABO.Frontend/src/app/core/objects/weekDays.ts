export enum WeekDaysEnum {
  Sunday = 'Domingo',
  Monday = 'Segunda-feira',
  Tuesday = 'Terça-feira',
  Wednesday = 'Quarta-feira',
  Thursday = 'Quinta-feira',
  Friday = 'Sexta-feira',
  Saturday = 'Sábado',
}

export const WEEKDAYS = Object.entries(WeekDaysEnum).map(([key, value]) => ({
  key,
  value,
}));
