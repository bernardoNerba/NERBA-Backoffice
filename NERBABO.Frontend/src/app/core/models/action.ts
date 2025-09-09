export interface Action {
  id: number;
  courseId: number;
  courseTitle: string;
  courseArea: string;
  courseMinHabilitationLevel: string;
  courseTotalDuration: number;
  courseDestinators: string[];
  courseModules: string[];
  coordenatorId: string;
  coordenatorName: string;
  title: string;
  actionNumber: number;
  administrationCode: string;
  address?: string;
  locality: string;
  weekDays: string[];
  startDate: string;
  endDate: string;
  status: string;
  regiment: string;
  paymentsProcessed: string;
  modulesHaveTeacher: string;
}

export interface ActionKpi {
  totalStudents: number;
  totalApproved: number;
  totalVolumeHours: number;
  totalVolumeDays: number;
}
