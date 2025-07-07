export enum StatusEnum {
  NotStarted = 'Não Iniciado',
  InProgress = 'Em Progresso',
  Completed = 'Concluído',
  Cancelled = 'Cancelado',
}

export const STATUS = Object.entries(StatusEnum).map(([key, value]) => ({
  key,
  value,
}));
