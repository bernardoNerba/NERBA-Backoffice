import { Component, Input, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MultiSelectModule } from 'primeng/multiselect';
import { ActionEnrollment } from '../../../../core/models/actionEnrollment';
import { MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Router } from '@angular/router';
import { ActionEnrollmentService } from '../../../../core/services/action-enrollment.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { InputTextModule } from 'primeng/inputtext';
import { BadgeModule } from 'primeng/badge';
import { TagModule } from 'primeng/tag';
import { ApprovalStatus } from '../../../enums/approval-status.enum';
import { UpsertActionEnrollmentComponent } from '../../../../features/enrollments/upsert-action-enrollment/upsert-action-enrollment.component';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';
import { DeleteActionEnrollmentsComponent } from '../../../../features/enrollments/delete-action-enrollments/delete-action-enrollments.component';

@Component({
  selector: 'app-action-enrollment-table',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    ButtonModule,
    MenuModule,
    MultiSelectModule,
    CommonModule,
    FormsModule,
    TruncatePipe,
    SpinnerComponent,
    InputTextModule,
    BadgeModule,
    TagModule,
    IconAnchorComponent,
  ],
  standalone: true,
  templateUrl: './action-enrollment-table.component.html',
})
export class ActionEnrollmentTableComponent implements OnInit, OnDestroy {
  @Input({ required: true }) actionEnrollments!: ActionEnrollment[];
  @Input({ required: true }) loading!: boolean;
  @Input() showActionColumn: boolean = false;
  @Input() showMenuActions: boolean = true;
  @Input({ required: true }) actionId?: number;
  @ViewChild('dt') dt!: Table;

  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedEnrollment: ActionEnrollment | undefined;
  first = 0;
  rows = 10;

  private subscriptions = new Subscription();

  constructor(
    private modalService: BsModalService,
    private router: Router,
    private actionEnrollmentService: ActionEnrollmentService
  ) {}

  ngOnInit(): void {
    if (this.showMenuActions) {
      this.menuItems = [
        {
          label: 'Opções',
          items: [
            {
              label: 'Detalhes do Formando',
              icon: 'pi pi-user',
              command: () => this.navigateToStudent(),
            },
            {
              label: 'Remover Inscrição',
              icon: 'pi pi-trash',
              command: () => this.onDeleteModal(this.selectedEnrollment!),
            },
          ],
        },
      ];
    }

    // Subscribe to service updates if needed
    this.subscriptions.add(
      this.actionEnrollmentService.updatedSource$.subscribe(
        (enrollmentId: number) => {
          this.refreshEnrollments(enrollmentId, 'update');
        }
      )
    );

    this.subscriptions.add(
      this.actionEnrollmentService.deletedSource$.subscribe(
        (enrollmentId: number) => {
          this.refreshEnrollments(enrollmentId, 'delete');
        }
      )
    );
  }

  refreshEnrollments(id: number, action: 'update' | 'delete'): void {
    if (id === 0) {
      // If id is 0, it indicates a full refresh is needed
      return;
    }

    const index = this.actionEnrollments.findIndex(
      (enrollment) => enrollment.enrollmentId === id
    );
    if (index === -1) return; // Enrollment not in this list, no action needed

    if (action === 'delete') {
      // Remove the enrollment from the list
      this.actionEnrollments = this.actionEnrollments.filter(
        (enrollment) => enrollment.enrollmentId !== id
      );
    } else if (action === 'update') {
      this.actionEnrollmentService.getById(id).subscribe({
        next: (updatedEnrollment) => {
          this.actionEnrollments[index] = updatedEnrollment;
          this.actionEnrollments = [...this.actionEnrollments];
        },
        error: (error) => {
          console.log('Failed to refresh enrollment: ', error);
        },
      });
    }
  }

  navigateToStudent(): void {
    if (this.selectedEnrollment?.personId) {
      this.router.navigate([
        '/people',
        this.selectedEnrollment.personId,
        'student',
      ]);
    } else {
      console.error(
        'Person ID not available for enrollment:',
        this.selectedEnrollment
      );
    }
  }

  onUpdateModal(enrollment: ActionEnrollment) {
    const initialState = {
      id: enrollment.enrollmentId,
      actionId: enrollment.actionId,
    };

    this.modalService.show(UpsertActionEnrollmentComponent, {
      initialState,
      class: 'modal-lg',
    });
  }

  onDeleteModal(enrollment: ActionEnrollment) {
    const initialState = {
      id: enrollment.enrollmentId,
      name: enrollment.studentFullName,
    };

    this.modalService.show(DeleteActionEnrollmentsComponent, {
      initialState,
      class: 'modal-md',
    });
  }

  next() {
    this.first = this.first + this.rows;
  }

  prev() {
    this.first = this.first - this.rows;
  }

  reset() {
    this.first = 0;
  }

  pageChange(event: any) {
    this.first = event.first;
    this.rows = event.rows;
  }

  isLastPage(): boolean {
    return this.actionEnrollments
      ? this.first + this.rows >= this.actionEnrollments.length
      : true;
  }

  isFirstPage(): boolean {
    return this.actionEnrollments ? this.first === 0 : true;
  }

  clearFilters() {
    this.searchValue = '';
    this.dt.reset();
  }

  onStudentEnrollment() {
    const initialState = {
      id: 0,
      actionId: this.actionId,
    };
    this.modalService.show(UpsertActionEnrollmentComponent, {
      initialState,
      class: 'modal-md',
    });
  }

  getApprovalStatusSeverity(
    approvalStatus: ApprovalStatus
  ): 'success' | 'warn' | 'danger' {
    switch (approvalStatus) {
      case ApprovalStatus.Approved:
        return 'success';
      case ApprovalStatus.Rejected:
        return 'danger';
      case ApprovalStatus.NotSpecified:
        return 'warn';
      default:
        return 'warn';
    }
  }

  getApprovalStatusIcon(approvalStatus: ApprovalStatus): string {
    switch (approvalStatus) {
      case ApprovalStatus.Approved:
        return 'pi pi-check';
      case ApprovalStatus.Rejected:
        return 'pi pi-times';
      case ApprovalStatus.NotSpecified:
      default:
        return 'pi pi-clock';
    }
  }

  // Keep backward compatibility for template
  getApprovalSeverity(approved: boolean): 'success' | 'warning' | 'danger' {
    return approved ? 'success' : 'warning';
  }

  getApprovalIcon(approved: boolean): string {
    return approved ? 'pi pi-check' : 'pi pi-clock';
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
