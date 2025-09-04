import { Component, Input, OnInit } from '@angular/core';
import { IUpsert } from '../../../core/interfaces/IUpsert';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  ActionEnrollment,
  CreateActionEnrollment,
  UpdateActionEnrollment,
} from '../../../core/models/actionEnrollment';
import { Student } from '../../../core/models/student';
import { Observable } from 'rxjs';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ActionEnrollmentService } from '../../../core/services/action-enrollment.service';
import { StudentsService } from '../../../core/services/students.service';
import { SharedService } from '../../../core/services/shared.service';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { DropdownModule } from 'primeng/dropdown';
import { CommonModule } from '@angular/common';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import {
  ApprovalStatus,
  ApprovalStatusOptions,
} from '../../../shared/enums/approval-status.enum';

@Component({
  selector: 'app-upsert-action-enrollment',
  imports: [
    ErrorCardComponent,
    DropdownModule,
    CommonModule,
    ReactiveFormsModule,
    InputTextModule,
    InputNumberModule,
  ],
  templateUrl: './upsert-action-enrollment.component.html',
})
export class UpsertActionEnrollmentComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: number; // enrollment id for update, 0 for create
  @Input({ required: true }) actionId!: number; // action id for the enrollment

  currentEnrollment?: ActionEnrollment | null;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  students$!: Observable<Student[]>;
  approvalStatusOptions = ApprovalStatusOptions;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  constructor(
    public bsModalRef: BsModalRef,
    private studentsService: StudentsService,
    private actionEnrollmentService: ActionEnrollmentService,
    private formBuilder: FormBuilder,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.initializeStudents();

    if (this.id !== 0) {
      this.isUpdate = true;
      this.actionEnrollmentService.getById(this.id).subscribe({
        next: (enrollment: ActionEnrollment) => {
          this.currentEnrollment = enrollment;
          this.patchFormValues();
        },
        error: (error: any) => {
          this.sharedService.showError('Inscrição não foi encontrada.');
          this.bsModalRef.hide();
        },
      });
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      studentId: ['', [Validators.required]],
      approvalStatus: [ApprovalStatus.NotSpecified, [Validators.required]],
    });
  }

  initializeStudents(): void {
    // For create mode, get only students not enrolled in this action
    if (!this.isUpdate && this.actionId) {
      this.students$ = this.studentsService.getAvailableForAction(
        this.actionId
      );
    } else {
      // For update mode, use all students
      this.students$ = this.studentsService.students$;
    }
  }

  patchFormValues(): void {
    console.log(this.currentEnrollment);
    if (this.currentEnrollment) {
      this.form.patchValue({
        studentId: this.currentEnrollment.studentId,
        approvalStatus: this.currentEnrollment.approvalStatus,
      });
    }
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessages = [];

    console.log(this.form.value);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    this.loading = true;

    const updateData: UpdateActionEnrollment = {
      id: this.id,
      actionId: this.actionId,
      studentId: this.form.value.studentId,
      approvalStatus: this.form.value.approvalStatus,
    };

    const createData: CreateActionEnrollment = {
      actionId: this.actionId,
      studentId: this.form.value.studentId,
    };

    this.actionEnrollmentService
      .upsert(this.isUpdate ? updateData : createData, this.isUpdate)
      .subscribe({
        next: (response) => {
          this.bsModalRef.hide();
          this.sharedService.showSuccess(response.message);
        },
        error: (error) => {
          this.errorMessages = this.sharedService.handleErrorResponse(error);
          this.loading = false;
        },
      });
  }

  getModalTitle(): string {
    return this.isUpdate ? 'Editar Inscrição' : 'Adicionar Formando';
  }

  getSubmitButtonText(): string {
    return this.isUpdate ? 'Atualizar' : 'Inscrever';
  }
}
