import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { SessionsService } from '../../../core/services/sessions.service';
import { Session, UpdateSession } from '../../../core/objects/sessions';
import { PresenceEnum, PRESENCES } from '../../../core/objects/presence';
import { MessageService } from 'primeng/api';
import { ICONS } from '../../../core/objects/icons';

// PrimeNG imports
import { TableModule, Table } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { SpinnerComponent } from '../spinner/spinner.component';
import { SharedService } from '../../../core/services/shared.service';

@Component({
  selector: 'app-teacher-presence',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    SelectModule,
    TagModule,
    ToastModule,
    ProgressSpinnerModule,
    TooltipModule,
    SpinnerComponent,
  ],
  providers: [MessageService],
  templateUrl: './teacher-presence.component.html',
})
export class TeacherPresenceComponent implements OnInit, OnDestroy {
  @Input({ required: true }) actionId!: number;
  @ViewChild('presenceTable') presenceTable!: Table;

  private destroy$ = new Subject<void>();

  sessions: Session[] = [];
  presenceForms: { [key: string]: FormGroup } = {};
  loading = false;
  saving = false;

  presenceOptions = PRESENCES;
  ICONS = ICONS;
  PresenceEnum = PresenceEnum;

  constructor(
    private sessionsService: SessionsService,
    private formBuilder: FormBuilder,
    private messageService: MessageService,
    private sharedService: SharedService
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
    // Listen to session updates
    this.sessionsService.updatedSource$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadData();
      });
  }

  private loadData(): void {
    this.loading = true;

    this.sessionsService
      .getSessionsByActionId(this.actionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (sessions: Session[]) => {
          this.sessions = sessions;
          this.createForms();
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading sessions:', error);
          this.sessions = [];
          this.loading = false;
          this.sharedService.showError('Erro ao carregar sessões.');
        },
      });
  }

  private createForms(): void {
    this.presenceForms = {};
    this.sessions.forEach((session) => {
      this.presenceForms[session.id] = this.formBuilder.group({
        teacherPresence: [session.teacherPresence || PresenceEnum.Unknown],
      });
    });
  }

  updateTeacherPresence(sessionId: number): void {
    const form = this.presenceForms[sessionId];
    if (!form || form.invalid) {
      return;
    }

    const session = this.sessions.find((s) => s.id === sessionId);
    if (!session) {
      return;
    }

    this.saving = true;

    const updateData: UpdateSession = {
      id: session.id,
      weekday: session.weekday,
      scheduledDate: session.scheduledDate,
      start: session.time.split(' - ')[0], // Extract start time from time range
      durationHours: session.durationHours,
      teacherPresence: form.value.teacherPresence,
      note: session.note,
    };

    this.sessionsService
      .update(updateData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'Presença do formador atualizada com sucesso.',
          });
          this.saving = false;
        },
        error: (error) => {
          console.error('Error updating teacher presence:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail: 'Erro ao atualizar presença do formador.',
          });
          this.saving = false;
        },
      });
  }

  getPresenceSeverity(presence: string): 'success' | 'danger' | 'secondary' {
    switch (presence) {
      case PresenceEnum.Present:
        return 'success';
      case PresenceEnum.Absent:
        return 'danger';
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
      default:
        return 'pi pi-exclamation-triangle';
    }
  }

  hasFormChanged(sessionId: number): boolean {
    const form = this.presenceForms[sessionId];
    const session = this.sessions.find((s) => s.id === sessionId);

    if (!form || !session) {
      return false;
    }

    return form.value.teacherPresence !== (session.teacherPresence || PresenceEnum.Unknown);
  }

  resetForm(sessionId: number): void {
    const form = this.presenceForms[sessionId];
    const session = this.sessions.find((s) => s.id === sessionId);

    if (form && session) {
      form.patchValue({
        teacherPresence: session.teacherPresence || PresenceEnum.Unknown,
      });
    }
  }

}