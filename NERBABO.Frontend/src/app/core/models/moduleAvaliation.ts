export interface AvaliationByModule {
  id: number;
  actionId: number;
  moduleId: number;
  moduleName: string;
  teacherPersonId: number;
  teacherName: string;
  gradings: GradingInfo[];
}

export interface UpdateModuleAvaliation {
  id: number;
  grade: number;
}

export interface GradingInfo {
  studentPersonId: number;
  studentName: string;
  grade: number;
  evaluated: boolean;
  moduleAvaliationId: number;
}
