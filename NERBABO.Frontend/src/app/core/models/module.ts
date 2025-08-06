export interface Module {
  id: number;
  name: string;
  hours: number;
  isActive: boolean;
  coursesQnt: number;
}

export interface ModuleTeacher {
  id: number;
  name: string;
  hours: number;
  isActive: boolean;
  coursesQnt: number;
  teacherId?: number;
  personId?: number;
  teacherName?: string;
}
