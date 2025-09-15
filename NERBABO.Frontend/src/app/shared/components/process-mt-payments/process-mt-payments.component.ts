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
import { ModuleTeachingService } from '../../../core/services/module-teaching.service';
import { ProcessModuleTeachingPayment } from '../../../core/models/moduleTeaching';
import { MessageService } from 'primeng/api';
import { ICONS } from '../../../core/objects/icons';

// PrimeNG imports
import { TableModule, Table } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { BadgeModule } from 'primeng/badge';
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
    SpinnerComponent,
  ],
  providers: [MessageService],
  templateUrl: './process-mt-payments.component.html',
})
export class ProcessMtPaymentsComponent implements OnInit, OnDestroy {
  @Input({ required: true }) actionId!: number;
  @ViewChild('paymentsTable') paymentsTable!: Table;

  private destroy$ = new Subject<void>();

  payments: ProcessModuleTeachingPayment[] = [];
  paymentForms: { [moduleId: number]: FormGroup } = {};
  loading = false;
  processing = false;
  activeIndex = -1;
  openAccordions: Set<number> = new Set();

  ICONS = ICONS;

  constructor(
    private moduleTeachingService: ModuleTeachingService,
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
    this.moduleTeachingService.updated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadData();
      });
  }

  private loadData(): void {
    this.loading = true;

    this.moduleTeachingService
      .getPaymentsByActionId(this.actionId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (payments: ProcessModuleTeachingPayment[]) => {
          this.payments = payments;
          this.createForms();
          this.loading = false;
          console.log(this.payments);
        },
        error: (error) => {
          console.error('Error loading module teaching payments:', error);
          this.sharedService.showError(error.error.detail);
          this.loading = false;
        },
      });
  }

  private createForms(): void {
    this.paymentForms = {};

    this.payments.forEach((payment) => {
      this.paymentForms[payment.moduleId] = this.formBuilder.group({
        moduleId: [payment.moduleId],
        moduleName: [payment.moduleTeacherName],
        paymentTotal: [
          payment.paymentTotal,
          [Validators.required, Validators.min(0)],
        ],
        calculatedTotal: [payment.calculatedTotal],
        paymentDate: [payment.paymentDate, [Validators.required]],
        isPayed: [payment.isPayed],
      });
    });
  }

  processPayment(moduleId: number): void {
    const form = this.paymentForms[moduleId];
    if (!form || form.invalid) {
      form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    this.processing = true;
    const formValue = form.value;

    this.moduleTeachingService
      .processPayment(moduleId, formValue)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.sharedService.showSuccess('Pagamento processado com sucesso');
          this.processing = false;
          this.loadDataWithStatePreservation();
        },
        error: (error) => {
          console.error('Error processing payment:', error);
          this.sharedService.showError(error.error.detail);
          this.processing = false;
        },
      });
  }

  getPaymentSeverity(isPayed: boolean): 'success' | 'warn' | 'danger' {
    return isPayed ? 'success' : 'warn';
  }

  getPaymentStatus(isPayed: boolean): 'Pago' | 'Pendente' {
    return isPayed ? 'Pago' : 'Pendente';
  }

  hasUnsavedChanges(moduleId: number): boolean {
    const form = this.paymentForms[moduleId];
    return form ? form.dirty : false;
  }

  resetForm(moduleId: number): void {
    const form = this.paymentForms[moduleId];
    if (form) {
      form.reset();
      this.loadDataWithStatePreservation();
    }
  }

  loadDataWithStatePreservation(): void {
    this.preserveAccordionState();
    this.loadData();
  }

  private preserveAccordionState(): void {
    this.openAccordions.clear();
    const openElements = document.querySelectorAll('.accordion-collapse.show');
    openElements.forEach((element) => {
      const moduleId = element.id.match(/module-(\d+)-content/);
      if (moduleId) {
        this.openAccordions.add(parseInt(moduleId[1]));
      }
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-PT', {
      style: 'currency',
      currency: 'EUR',
    }).format(value);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('pt-PT');
  }
}
