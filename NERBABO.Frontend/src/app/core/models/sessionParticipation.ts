export interface SessionParticipation {
  sessionParticipationId: number;
  sessionId: number;
  actionEnrollmentId: number;
  presence: string;
  attendance: number;
  attendanceHours: number;
  attendanceMinutes: number;
}

export interface CreateSessionParticipation {
  sessionId: number;
  actionEnrollmentId: number;
  presence: string;
  attendance: number;
}

export interface UpdateSessionParticipation {
  sessionParticipationId: number;
  sessionId: number;
  actionEnrollmentId: number;
  presence: string;
  attendance: number;
}

export interface UpsertSessionAttendance {
  sessionId: number;
  studentAttendances: StudentAttendance[];
}

export interface StudentAttendance {
  actionEnrollmentId: number;
  studentName: string;
  presence: string;
  attendance: number;
}

export interface SessionWithAttendance {
  sessionId: number;
  sessionDate: string;
  sessionTime: string;
  moduleName: string;
  teacherName: string;
  durationHours: number;
  attendances: StudentAttendanceForDisplay[];
}

export interface StudentAttendanceForDisplay {
  actionEnrollmentId: number;
  studentName: string;
  presence: string;
  attendance: number;
  attendanceHours: number;
  attendanceMinutes: number;
  sessionParticipationId?: number;
}