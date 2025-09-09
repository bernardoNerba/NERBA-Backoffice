import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormsModule,
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { SessionsService } from '../../../core/services/sessions.service';
import { SessionParticipationService } from '../../../core/services/session-participation.service';
import { ActionEnrollmentService } from '../../../core/services/action-enrollment.service';
import { Session } from '../../../core/objects/sessions';
import { ActionEnrollment } from '../../../core/models/actionEnrollment';
import {
  SessionWithAttendance,
  StudentAttendanceForDisplay,
  UpsertSessionAttendance,
} from '../../../core/models/sessionParticipation';
import { PresenceEnum, PRESENCES } from '../../../core/objects/presence';
import { MessageService } from 'primeng/api';
import { ICONS } from '../../../core/objects/icons';

// PrimeNG imports
import { TableModule, Table } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { BadgeModule } from 'primeng/badge';
import { SpinnerComponent } from '../spinner/spinner.component';

@Component({
  selector: 'app-session-attendance',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    SelectModule,
    InputNumberModule,
    TagModule,
    ToastModule,
    ProgressSpinnerModule,
    BadgeModule,
    SpinnerComponent,
  ],
  providers: [MessageService],
  templateUrl: './session-attendance.component.html',
})
export class SessionAttendanceComponent implements OnInit, OnDestroy {
  // Custom validator for total attendance time
  static attendanceTimeValidator(maxDuration: number): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      if (!(group instanceof FormGroup)) {
        return null;
      }

      const hours = group.get('attendanceHours')?.value || 0;
      const minutes = group.get('attendanceMinutes')?.value || 0;
      const presence = group.get('presence')?.value;

      // Only validate if student is marked as present
      if (presence !== PresenceEnum.Present) {
        return null;
      }

      const totalTime = hours + minutes / 60;

      if (totalTime > maxDuration) {
        return {
          totalAttendanceExceeded: {
            max: maxDuration,
            actual: totalTime,
            message: `Total de frequência (${Math.floor(
              totalTime
            )}h ${Math.round(
              (totalTime % 1) * 60
            )}min) não pode exceder a duração da sessão (${Math.floor(
              maxDuration
            )}h ${Math.round((maxDuration % 1) * 60)}min)`,
          },
        };
      }

      return null;
    };
  }
  @Input({ required: true }) actionId!: number;
  @ViewChild('sessionTable') sessionTable!: Table;

  private destroy$ = new Subject<void>();

  sessionsWithAttendance: SessionWithAttendance[] = [];
  actionEnrollments: ActionEnrollment[] = [];
  presenceOptions = PRESENCES;
  attendanceForms: { [sessionId: number]: FormGroup } = {};
  loading = false;
  saving = false;
  activeIndex = -1; // For accordion control
  openAccordions: Set<number> = new Set(); // Track which accordions are open

  ICONS = ICONS;
  PresenceEnum = PresenceEnum;

  constructor(
    private sessionsService: SessionsService,
    private sessionParticipationService: SessionParticipationService,
    private actionEnrollmentService: ActionEnrollmentService,
    private formBuilder: FormBuilder,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadData();
    this.setupSubscriptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSubscriptions(): void {
    this.sessionParticipationService.createdSource$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadData();
      });

    this.sessionParticipationService.updatedSource$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadData();
      });
  }

  private loadData(): void {
    this.loading = true;

    forkJoin({
      sessions: this.sessionsService.getSessionsByActionId(this.actionId),
      enrollments: this.actionEnrollmentService.getByActionId(this.actionId),
      participations: this.sessionParticipationService.getByActionId(
        this.actionId
      ),
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ sessions, enrollments, participations }) => {
          this.actionEnrollments = enrollments;
          this.processSessionsData(sessions, participations);
          this.createForms();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading session attendance data:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail: 'Erro ao carregar dados de presenças',
          });
          this.loading = false;
        },
      });
  }

  private processSessionsData(
    sessions: Session[],
    participations: any[]
  ): void {
    // Sort sessions by scheduled date (oldest first)
    const sortedSessions = sessions.sort(
      (a, b) =>
        new Date(a.scheduledDate).getTime() -
        new Date(b.scheduledDate).getTime()
    );

    this.sessionsWithAttendance = sortedSessions.map((session) => {
      const sessionParticipations = participations.filter(
        (p) => p.sessionId === session.id
      );

      const attendances: StudentAttendanceForDisplay[] =
        this.actionEnrollments.map((enrollment) => {
          const participation = sessionParticipations.find(
            (p) => p.actionEnrollmentId === enrollment.enrollmentId
          );

          return {
            actionEnrollmentId: enrollment.enrollmentId,
            studentName: enrollment.studentFullName,
            presence: participation?.presence || PresenceEnum.Unknown,
            attendance: participation?.attendance || 0,
            attendanceHours: participation?.attendanceHours || 0,
            attendanceMinutes: participation?.attendanceMinutes || 0,
            sessionParticipationId: participation?.sessionParticipationId,
          };
        });

      return {
        sessionId: session.id,
        sessionDate: session.scheduledDate,
        sessionTime: session.time,
        moduleName: session.moduleName,
        teacherName: session.teacherPersonName,
        durationHours: session.durationHours,
        attendances,
      };
    });
  }

  private createForms(): void {
    this.attendanceForms = {};

    this.sessionsWithAttendance.forEach((session) => {
      const studentControls = session.attendances.map((attendance) =>
        this.formBuilder.group(
          {
            actionEnrollmentId: [attendance.actionEnrollmentId],
            studentName: [attendance.studentName],
            presence: [attendance.presence],
            attendanceHours: [
              attendance.attendanceHours,
              [Validators.min(0), Validators.max(session.durationHours)],
            ],
            attendanceMinutes: [
              attendance.attendanceMinutes,
              [Validators.min(0), Validators.max(59)],
            ],
            sessionParticipationId: [attendance.sessionParticipationId],
          },
          {
            validators: [
              SessionAttendanceComponent.attendanceTimeValidator(
                session.durationHours
              ),
            ],
          }
        )
      );

      this.attendanceForms[session.sessionId] = this.formBuilder.group({
        sessionId: [session.sessionId],
        students: this.formBuilder.array(studentControls),
      });
    });
  }

  getStudentsFormArray(sessionId: number): FormArray {
    return this.attendanceForms[sessionId]?.get('students') as FormArray;
  }

  // Helper methods to convert between decimal and hour/minute format
  private convertDecimalToHoursMinutes(decimal: number): {
    hours: number;
    minutes: number;
  } {
    const hours = Math.floor(decimal);
    const minutes = Math.round((decimal - hours) * 60);
    return { hours, minutes };
  }

  private convertHoursMinutesToDecimal(hours: number, minutes: number): number {
    return hours + minutes / 60;
  }

  // Check if attendance is less than half of session duration
  isAttendanceLowForSession(sessionId: number, studentIndex: number): boolean {
    const session = this.sessionsWithAttendance.find(
      (s) => s.sessionId === sessionId
    );
    if (!session) return false;

    const studentsArray = this.getStudentsFormArray(sessionId);
    const studentControl = studentsArray.at(studentIndex);

    const hours = studentControl.get('attendanceHours')?.value || 0;
    const minutes = studentControl.get('attendanceMinutes')?.value || 0;
    const presence = studentControl.get('presence')?.value;

    if (presence !== PresenceEnum.Present) return false;

    const totalAttendance = this.convertHoursMinutesToDecimal(hours, minutes);
    const halfSessionDuration = session.durationHours / 2;

    return totalAttendance > 0 && totalAttendance < halfSessionDuration;
  }

  saveSessionAttendance(sessionId: number): void {
    const form = this.attendanceForms[sessionId];
    if (!form || form.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Aviso',
        detail: 'Por favor, verifique os dados inseridos',
      });
      return;
    }

    this.saving = true;
    const formValue = form.value;

    const upsertData: UpsertSessionAttendance = {
      sessionId: sessionId,
      studentAttendances: formValue.students.map((student: any) => ({
        actionEnrollmentId: student.actionEnrollmentId,
        studentName: student.studentName,
        presence: student.presence,
        attendance: this.convertHoursMinutesToDecimal(
          student.attendanceHours || 0,
          student.attendanceMinutes || 0
        ),
      })),
    };

    this.sessionParticipationService
      .upsertSessionAttendance(upsertData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'Presenças guardadas com sucesso',
          });
          this.saving = false;
          this.loadDataWithStatePreservation();
        },
        error: (error) => {
          console.error('Error saving session attendance:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail: 'Erro ao guardar presenças',
          });
          this.saving = false;
        },
      });
  }

  getPresenceSeverity(
    presence: string
  ): 'success' | 'warning' | 'danger' | 'secondary' {
    switch (presence) {
      case PresenceEnum.Present:
        return 'success';
      case PresenceEnum.Absent:
        return 'danger';
      case PresenceEnum.Unknown:
      default:
        return 'secondary';
    }
  }

  getPresenceIcon(presence: string): string {
    switch (presence) {
      case PresenceEnum.Present:
        return 'pi pi-check';
      case PresenceEnum.Absent:
        return 'pi pi-times';
      case PresenceEnum.Unknown:
      default:
        return 'pi pi-question';
    }
  }

  onPresenceChange(
    sessionId: number,
    studentIndex: number,
    presence: string
  ): void {
    const studentsArray = this.getStudentsFormArray(sessionId);
    const studentControl = studentsArray.at(studentIndex);

    // Only clear attendance when marked as absent, don't auto-fill when present
    if (presence === PresenceEnum.Absent) {
      studentControl.patchValue({
        presence: presence,
        attendanceHours: 0,
        attendanceMinutes: 0,
      });
    } else {
      studentControl.patchValue({
        presence: presence,
      });
    }
  }

  hasUnsavedChanges(sessionId: number): boolean {
    const form = this.attendanceForms[sessionId];
    return form ? form.dirty : false;
  }

  resetForm(sessionId: number): void {
    const form = this.attendanceForms[sessionId];
    if (form) {
      form.reset();
      this.loadDataWithStatePreservation(); // Reload original data while preserving accordion state
    }
  }

  loadDataWithStatePreservation(): void {
    // Store current accordion state before reloading data
    this.preserveAccordionState();
    this.loadData();
  }

  private preserveAccordionState(): void {
    // Capture which accordions are currently open
    this.openAccordions.clear();
    const openElements = document.querySelectorAll('.accordion-collapse.show');
    openElements.forEach((element) => {
      const sessionId = element.id.match(/session-(\d+)-content/);
      if (sessionId) {
        this.openAccordions.add(parseInt(sessionId[1]));
      }
    });
  }

  getSessionDurationHours(sessionId: number): number {
    const session = this.sessionsWithAttendance.find(
      (s) => s.sessionId === sessionId
    );
    return session ? session.durationHours : 24;
  }

  // Check if student has attendance time validation error
  hasAttendanceTimeError(sessionId: number, studentIndex: number): boolean {
    const studentsArray = this.getStudentsFormArray(sessionId);
    const studentControl = studentsArray.at(studentIndex);
    return studentControl?.hasError('totalAttendanceExceeded') ?? false;
  }

  // Get attendance time validation error message
  getAttendanceTimeErrorMessage(
    sessionId: number,
    studentIndex: number
  ): string {
    const studentsArray = this.getStudentsFormArray(sessionId);
    const studentControl = studentsArray.at(studentIndex);
    const error = studentControl?.getError('totalAttendanceExceeded');
    return error?.message || '';
  }

  // Check if all attendances for a session have presence set (not Unknown)
  isSessionPresenceComplete(sessionId: number): boolean {
    const session = this.sessionsWithAttendance.find(
      (s) => s.sessionId === sessionId
    );
    if (!session || session.attendances.length === 0) return false;

    const form = this.attendanceForms[sessionId];
    if (!form) return false;

    const studentsArray = this.getStudentsFormArray(sessionId);
    
    // Check if all students have presence set (not Unknown)
    for (let i = 0; i < studentsArray.length; i++) {
      const studentControl = studentsArray.at(i);
      const presence = studentControl.get('presence')?.value;
      if (presence === PresenceEnum.Unknown) {
        return false;
      }
    }
    return true;
  }
}
