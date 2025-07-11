export interface ActionForm {
  id: number;
  courseId: number;
  title: string;
  administrationCode: string;
  address?: string;
  locality: string;
  weekDays: string[];
  startDate: string;
  endDate: string;
  status: string;
  regiment: string;
}
