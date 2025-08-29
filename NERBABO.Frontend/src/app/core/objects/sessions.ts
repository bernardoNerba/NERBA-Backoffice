export interface Session {
  id: number;
  moduleTeachingId: number;
  moduleName: string;
  teacherPersonId: number;
  teacherPersonName: string;
  coordenatorPersonId: number;
  coordenatorPersonName: string;
  scheduledDate: string;
  weekday: string;
  time: string;
  durationHours: number;
  teacherPresence: string;
  note: string;
}

export interface CreateSession {
  moduleTeachingId: number;
  weekday: string;
  scheduledDate: string;
  start: string;
  durationHours: number;
  note: string;
}

export interface UpdateSession {
  id: number;
  weekday: string;
  scheduledDate: string;
  start: string;
  durationHours: number;
  teacherPresence: string;
  note: string;
}
