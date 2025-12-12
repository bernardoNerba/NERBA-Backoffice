export type Notification = {
  id: number;
  title: string;
  message: string;
  type: string;
  status: string;
  relatedPersonId?: number;
  relatedPersonName?: string;
  relatedEntityType?: string;
  relatedEntityId?: number;
  actionUrl?: string;
  createdAt: string;
  readAt?: string;
  readByUserId?: string;
};

export type NotificationCount = {
  totalCount: number;
  unreadCount: number;
};

export enum NotificationTypeEnum {
  MissingDocument = 'Documento em Falta',
  IncompleteInformation = 'Informação Incompleta',
  GeneralAlert = 'Alerta Geral',
}

export enum NotificationStatusEnum {
  Unread = 'Não Lida',
  Read = 'Lida',
  Archived = 'Arquivada',
}
