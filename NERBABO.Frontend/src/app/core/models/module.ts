export interface Module {
  id: number;
  name: string;
  hours: number;
  isActive: boolean;
  coursesQnt: number;
}

export interface RetrievedModule {
  id: number;
  categoryId: number;
  name: string;
  hours: number;
  isActive: boolean;
  coursesQnt: number;
  categoryName: string;
  categoryShortenName: string;
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
