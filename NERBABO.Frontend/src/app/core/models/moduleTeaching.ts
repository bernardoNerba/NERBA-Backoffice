export interface ModuleTeaching {
  id: number;
  actionId: number;
  moduleId: number;
  teacherId: number;
  avaliationCoordenator: number;
  avaliationStudents: number;
  avaliationAvg: number;
  paymentTotal: number;
  paymentDate: string;
  paymentProcessed: boolean;

  // Navigation properties - these would be populated when included
  teacherName?: string;
  teacherEmail?: string;
  teacherPhone?: string;
  moduleName?: string;
  moduleHours?: number;
}

export interface CreateModuleTeaching {
  teacherId: number;
  actionId: number;
  moduleId: number;
}

export interface UpdateModuleTeaching {
  id: number;
  teacherId: number;
  actionId: number;
  moduleId: number;
}
