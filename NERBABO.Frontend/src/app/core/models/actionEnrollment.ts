import { ApprovalStatus } from '../../shared/enums/approval-status.enum';

export interface ActionEnrollment {
  enrollmentId: number;
  evaluation: number;
  studentFullName: string;
  approvalStatus: ApprovalStatus;
  approved: boolean;
  actionId: number;
  personId: number;
  studentId: number;
  createdAt: string; // API returns string, not Date
}

export interface CreateActionEnrollment {
  actionId: number;
  studentId: number;
}

export interface UpdateActionEnrollment {
  id: number;
  actionId: number;
  studentId: number;
  approvalStatus: ApprovalStatus;
}
