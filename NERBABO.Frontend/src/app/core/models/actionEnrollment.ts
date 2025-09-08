import { ApprovalStatus } from '../../shared/enums/approval-status.enum';

export interface ActionEnrollment {
  enrollmentId: number;
  studentFullName: string;
  approvalStatus: ApprovalStatus;
  avgEvaluation: number;
  actionId: number;
  StudentAvaliated: boolean;
  personId: number;
  studentId: number;
  createdAt: string;
}

export interface CreateActionEnrollment {
  actionId: number;
  studentId: number;
}

export interface UpdateActionEnrollment {
  id: number;
  actionId: number;
  studentId: number;
}
