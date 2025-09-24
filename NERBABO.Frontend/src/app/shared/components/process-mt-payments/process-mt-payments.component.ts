import { Component, Input, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormsModule,
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
} from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { PaymentsService, TeacherPayment, UpdateTeacherPayment } from '../../../core/services/payments.service';
import { MessageService } from 'primeng/api';
import { ICONS } from '../../../core/objects/icons';
import { convertDateOnlyToPtDate, matchDateOnly } from '../../utils';

// PrimeNG imports
import { TableModule, Table } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { SpinnerComponent } from '../spinner/spinner.component';
import { SharedService } from '../../../core/services/shared.service';

@Component({
  selector: 'app-process-mt-payments',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    InputNumberModule,
    TagModule,
    ToastModule,
    ProgressSpinnerModule,
    BadgeModule,
    TooltipModule,
    SpinnerComponent,
  ],
  providers: [MessageService],
  templateUrl: './process-mt-payments.component.html',
})
export class ProcessMtPaymentsComponent implements OnInit, OnDestroy {
  @Input({ required: true }) actionId!: number;
  @ViewChild('paymentsTable') paymentsTable!: Table;

  private destroy$ = new Subject<void>();

  payments: TeacherPayment[] = [];
  paymentForms: { [key: string]: FormGroup } = {};
  loading = false;
  processing = false;
  savingAll = false;

  ICONS = ICONS;

  constructor(
    private paymentsService: PaymentsService,
    private formBuilder: FormBuilder,
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
    this.paymentsService.updated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadData();
      });
  }

  private loadData(): void {
    this.loading = true;

    this.paymentsService
      .getTeacherPaymentsByActionId(this.actionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (payments: TeacherPayment[]) => {
          this.payments = payments;
          this.createForms();
          this.loading = false;
          console.log(this.payments);
        },
        error: (error) => {
          console.error('Error loading teacher payments:', error);
          this.loading = false;
        },
      });
  }

  private createForms(): void {
    this.paymentForms = {};

    this.payments.forEach((payment) => {
      const key = `${payment.moduleId}-${payment.teacherPersonId}`;
      this.paymentForms[key] = this.formBuilder.group({
        moduleTeachingId: [payment.moduleTeachingId],
        paymentTotal: [
          payment.paymentTotal,
          [Validators.required, Validators.min(0)],
        ],
        paymentDate: [this.getFormattedPaymentDate(payment.paymentDate), [Validators.required]],
      });
    });
  }


  getPaymentSeverity(isProcessed: boolean): 'success' | 'warn' | 'danger' {
    return isProcessed ? 'success' : 'warn';
  }

  getPaymentStatus(isProcessed: boolean): 'Pago' | 'Pendente' {
    return isProcessed ? 'Pago' : 'Pendente';
  }

  hasUnsavedChanges(payment: TeacherPayment): boolean {
    const key = `${payment.moduleId}-${payment.teacherPersonId}`;
    const form = this.paymentForms[key];
    return form ? form.dirty : false;
  }


  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-PT', {
      style: 'currency',
      currency: 'EUR',
    }).format(value);
  }

  formatDate(dateString: string): string {
    // If the date is already in dd/MM/yyyy format, return as is
    // If it's in yyyy-MM-dd format, convert to dd/MM/yyyy
    if (dateString.includes('/')) {
      return dateString;
    }
    return convertDateOnlyToPtDate(dateString);
  }

  getFormattedPaymentDate(paymentDate: string): string {
    // Check if the date is null, empty, or the default date (01/01/0001)
    if (!paymentDate || paymentDate === '01/01/0001' || paymentDate.includes('0001')) {
      return ''; // Return empty string for HTML date input
    }
    return matchDateOnly(paymentDate);
  }

  getTotalCalculated(): number {
    return this.payments.reduce((total, payment) => total + payment.calculatedTotal, 0);
  }

  hasAnyUnsavedChanges(): boolean {
    return this.payments.some(payment => this.hasUnsavedChanges(payment));
  }

  resetAllForms(): void {
    Object.keys(this.paymentForms).forEach(key => {
      const form = this.paymentForms[key];
      if (form && form.dirty) {
        form.reset();
      }
    });
    this.loadData();
  }

  saveAllChanges(): void {
    if (!this.hasAnyUnsavedChanges()) {
      this.sharedService.showInfo('Não há alterações para guardar.');
      return;
    }

    const changedPayments = this.payments.filter(payment => this.hasUnsavedChanges(payment));
    const invalidForms = changedPayments.filter(payment => {
      const key = `${payment.moduleId}-${payment.teacherPersonId}`;
      const form = this.paymentForms[key];
      return form && form.invalid;
    });

    if (invalidForms.length > 0) {
      changedPayments.forEach(payment => {
        const key = `${payment.moduleId}-${payment.teacherPersonId}`;
        const form = this.paymentForms[key];
        if (form && form.invalid) {
          form.markAllAsTouched();
        }
      });
      this.sharedService.showError(
        `Existem ${invalidForms.length} formulário(s) com dados inválidos. Por favor, corrija os erros antes de guardar.`
      );
      return;
    }

    this.savingAll = true;
    let savedCount = 0;
    let errorCount = 0;

    changedPayments.forEach((payment, index) => {
      const key = `${payment.moduleId}-${payment.teacherPersonId}`;
      const form = this.paymentForms[key];
      if (!form) return;

      const formValue = form.value;
      const updatePayment: UpdateTeacherPayment = {
        moduleTeachingId: formValue.moduleTeachingId,
        paymentTotal: formValue.paymentTotal,
        paymentDate: formValue.paymentDate, // HTML date input provides yyyy-MM-dd format that backend expects
      };

      this.paymentsService
        .updateTeacherPayments(updatePayment)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            savedCount++;
            if (savedCount + errorCount === changedPayments.length) {
              this.finalizeBulkSave(savedCount, errorCount);
            }
          },
          error: (error) => {
            console.error('Error processing payment:', error);
            errorCount++;
            if (savedCount + errorCount === changedPayments.length) {
              this.finalizeBulkSave(savedCount, errorCount);
            }
          },
        });
    });
  }

  private finalizeBulkSave(savedCount: number, errorCount: number): void {
    this.savingAll = false;

    if (errorCount === 0) {
      this.sharedService.showSuccess(`${savedCount} pagamento(s) guardado(s) com sucesso.`);
    } else if (savedCount > 0) {
      this.sharedService.showWarning(
        `${savedCount} pagamento(s) guardado(s) com sucesso. ${errorCount} pagamento(s) falharam.`
      );
    } else {
      this.sharedService.showError(
        `Falha ao guardar todos os pagamentos. ${errorCount} erro(s) ocorreram.`
      );
    }

    this.loadData();
  }

  getModifiedFormsCount(): number {
    return this.payments.filter(payment => this.hasUnsavedChanges(payment)).length;
  }
}
