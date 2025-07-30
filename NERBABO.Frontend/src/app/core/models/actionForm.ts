export interface ActionForm {
  id: number;
  courseId: number;
  administrationCode: string;
  address?: string;
  locality: string;
  weekDays: string[];
  startDate: string;
  endDate: string;
  status: string;
  regiment: string;
}
