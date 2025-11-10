export interface Module {
  id: number;
  name: string;
  hours: number;
  isActive: boolean;
  coursesQnt: number;
}

export interface RetrievedModule {
  id: number;
  name: string;
  hours: number;
  isActive: boolean;
  coursesQnt: number;
  allDifferentCategories: string;
  categories: Array<number>;
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
