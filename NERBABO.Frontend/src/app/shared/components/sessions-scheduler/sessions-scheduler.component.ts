import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { SessionsService } from '../../../core/services/sessions.service';
import { Session } from '../../../core/objects/sessions';
import { BsModalService } from 'ngx-bootstrap/modal';
import { SharedService } from '../../../core/services/shared.service';
import { formatDateForApi, getWeekDayPT } from '../../utils';
import { Subscription } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { DatePickerModule } from 'primeng/datepicker';
import { CardModule } from 'primeng/card';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Tag } from 'primeng/tag';
import { UpsertSessionsComponent } from '../../../features/sessions/upsert-sessions/upsert-sessions.component';
import { Action } from '../../../core/models/action';
import { DeleteSessionsComponent } from '../../../features/sessions/delete-sessions/delete-sessions.component';

@Component({
  selector: 'app-sessions-scheduler',
  imports: [
    ButtonModule,
    DatePickerModule,
    CommonModule,
    CardModule,
    FormsModule,
  ],
  templateUrl: './sessions-scheduler.component.html',
  styleUrl: './sessions-scheduler.component.css',
})
export class SessionsSchedulerComponent implements OnInit {
  @Input({ required: true }) query!: 'all' | 'byActionId';
  @Input({}) action?: Action | null;
  sessions: Session[] = [];
  filteredSessions: Session[] = [];
  selectedDate?: Date;
  sessionsDates: string[] = [];
  totalRegisteredHours: number = 0;

  private subscriptions: Subscription = new Subscription();

  constructor(
    private sessionsService: SessionsService,
    private modalService: BsModalService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.loadSessions();
  }

  loadSessions(): void {
    switch (this.query) {
      case 'byActionId':
        if (this.action?.id) {
          this.sessionsService
            .getSessionsByActionId(this.action?.id)
            .subscribe({
              next: (sessions: Session[]) => {
                console.log('Loaded sessions:', sessions);
                this.sessions = sessions;
                this.filteredSessions = this.sessions;
                this.sessionsDates = sessions.map((s) => s.scheduledDate);
                console.log('Session dates for calendar:', this.sessionsDates);
              },
              error: (error: any) => {
                console.error('Error loading sessions:', error);
                this.sessions = [];
                this.filteredSessions = [];
              },
            });
        } else {
          this.sharedService.showWarning(
            'A ação não foi passada para filtrar as sessões'
          );
          this.sessions = [];
          this.filteredSessions = [];
        }
        break;
      case 'all':
        // TODO: fetch all sessions
        break;
    }
  }

  calculateTotalRegisteredHours(): void {
    this.totalRegisteredHours = this.sessions.reduce((total, session) => {
      return total + session.durationHours;
    }, 0);
  }

  onDateSelect(event: any): void {
    this.selectedDate = event;
    this.filterSessionsByDate();
  }

  filterSessionsByDate(): void {
    if (this.selectedDate) {
      const selectedDateString = formatDateForApi(new Date(this.selectedDate));
      this.filteredSessions = this.sessions.filter(
        (session) => session.scheduledDate === selectedDateString
      );
    } else {
      this.filteredSessions = this.sessions;
    }
  }

  clearDateSelection(): void {
    this.selectedDate = undefined;
    this.filteredSessions = this.sessions;
  }

  onCreateSession(): void {
    this.modalService.show(UpsertSessionsComponent, {
      class: 'modal-lg',
      initialState: {
        id: 0, // New session
        actionId: this.action?.id,
        actionTitle: this.action?.title,
        selectedDate: this.selectedDate,
      },
    });
  }

  onEditSession(session: Session): void {
    // this.modalService.show(UpsertSessionsComponent, {
    //   class: 'modal-lg',
    //   initialState: {
    //     id: session.id,
    //     actionId: this.action?.id,
    //     actionTitle: this.action?.title,
    //   },
    // });
  }

  onDeleteSession(session: Session): void {
    this.modalService.show(DeleteSessionsComponent, {
      class: 'modal-md',
      initialState: {
        id: session.id,
        session: session,
      },
    });
  }

  isSessionDay(date: any): boolean {
    const dateString = formatDateForApi(
      new Date(date.year, date.month, date.day)
    );
    const isSession = this.sessionsDates.includes(dateString);
    return isSession;
  }

  isValidActionDay(date: any): boolean {
    if (!this.action) return false;

    const currentDate = new Date(date.year, date.month, date.day);
    const startDate = new Date(this.action.startDate);
    const endDate = new Date(this.action.endDate);

    // Check if date is within action date range
    if (currentDate < startDate || currentDate > endDate) {
      return false;
    }

    // Check if current weekday is allowed
    const currentWeekDay = getWeekDayPT(currentDate);

    return this.action.weekDays.some(
      (allowedDay) => allowedDay.toLowerCase() === currentWeekDay
    );
  }

  getPresenceSeverity(presence: string): string {
    switch (presence) {
      case 'Não Especificado':
        return 'secondary';
      case 'Presente':
        return 'success';
      case 'Faltou':
        return 'danger';
      default:
        return 'secondary';
    }
  }

  onScheduledDateClicked(date: string) {
    this.selectedDate = new Date(date);
    this.filterSessionsByDate();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
