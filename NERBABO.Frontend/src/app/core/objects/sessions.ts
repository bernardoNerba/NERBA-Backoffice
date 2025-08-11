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
}

// "id": 6,
//         "moduleTeachingId": 3,
//         "moduleId": 2,
//         "moduleName": "Módulo Lorem",
//         "teacherPersonId": 2,
//         "teacherPersonName": "Dinis dos Anjos Ferreira",
//         "coordenatorPersonId": 1,
//         "coordenatorPersonName": "Admin Admin",
//         "scheduledDate": "2025-09-19",
//         "time": "09:00 - 10:00",
//         "durationHours": 1,
//         "teacherPresence": "Não Especificado"
