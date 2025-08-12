import { Component, Input, OnInit } from '@angular/core';
import { IUpsert } from '../../../core/interfaces/IUpsert';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { SessionsService } from '../../../core/services/sessions.service';
import { SharedService } from '../../../core/services/shared.service';
import { CreateSession, Session } from '../../../core/objects/sessions';
import { ModuleTeachingService } from '../../../core/services/module-teaching.service';
import { BsModalRef } from 'ngx-bootstrap/modal';
import {
  MinimalModuleTeaching,
  ModuleTeaching,
} from '../../../core/models/moduleTeaching';
import { CommonModule } from '@angular/common';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { DropdownModule } from 'primeng/dropdown';
import { DatePickerModule } from 'primeng/datepicker';
import { InputTextModule } from 'primeng/inputtext';
import { Subscription } from 'rxjs';
import { formatDateForApi, stringToTimeObject } from '../../../shared/utils';
import { Select } from 'primeng/select';

@Component({
  selector: 'app-upsert-sessions',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ErrorCardComponent,
    DropdownModule,
    DatePickerModule,
    InputTextModule,
    Select,
  ],
  templateUrl: './upsert-sessions.component.html',
})
export class UpsertSessionsComponent implements OnInit, IUpsert {
  @Input({}) id!: number;
  @Input({}) actionId!: number;
  @Input({}) actionTitle!: string;
  @Input({}) selectedDate: Date | undefined;
  currentSesssion?: Session | null;
  moduleTeachingOptions: MinimalModuleTeaching[] = [];
  endTime: string = '';

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;

  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});

  private subscription: Subscription = new Subscription();

  constructor(
    private sessionsService: SessionsService,
    private sharedService: SharedService,
    private moduleTeachingService: ModuleTeachingService,
    private formBuilder: FormBuilder,
    public bsModalRef: BsModalRef
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.initializeData();
    this.listenToFormChanges();
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      moduleTeachingId: ['', [Validators.required]],
      weekday: [''],
      scheduledDate: ['', Validators.required],
      start: ['', [Validators.required]],
      durationHours: [
        '00:00',
        [Validators.required, Validators.min(1), Validators.max(12)],
      ],
      teacherPresence: [false],
    });
  }

  initializeData(): void {
    // initializes Module Teachings options from dropdown
    console.log('ActionId:', this.actionId);
    if (this.actionId) {
      this.moduleTeachingService
        .getModuleTeachingByActionMinimal(this.actionId)
        .subscribe({
          next: (values: MinimalModuleTeaching[]) => {
            console.log('Module teaching options received:', values);
            if (Array.isArray(values)) {
              this.moduleTeachingOptions = values;
            } else if (values && typeof values === 'object') {
              // API returned a single object instead of array
              this.moduleTeachingOptions = [values as MinimalModuleTeaching];
            } else {
              this.moduleTeachingOptions = [];
            }
          },
          error: (error: any) => {
            console.error('Error fetching module teachings:', error);
            this.moduleTeachingOptions = [];
          },
        });
    } else {
      console.error('ActionId is not provided');
      this.moduleTeachingOptions = [];
    }
  }

  patchFormValues(): void {
    if (this.currentSesssion) {
      this.form.patchValue({
        moduleTeachingId: this.currentSesssion.moduleTeachingId,
        weekday: this.currentSesssion.weekday,
        scheduledDate: ['', Validators.required],
        start: this.currentSesssion.time.split(' - ')[0],
        durationHours: this.currentSesssion.durationHours,
        teacherPresence: this.currentSesssion.teacherPresence,
      });
    }
  }

  private listenToFormChanges(): void {
    this.subscription.add(
      this.form.valueChanges.subscribe((formValue) => {
        if (formValue.start && formValue.durationHours) {
          const start = stringToTimeObject(formValue.start);
          const duration = stringToTimeObject(formValue.durationHours);

          const result = new Date(start);
          result.setHours(result.getHours() + duration.getHours());
          result.setMinutes(result.getMinutes() + duration.getMinutes());

          this.endTime = `${result
            .getHours()
            .toString()
            .padStart(2, '0')}:${result
            .getMinutes()
            .toString()
            .padStart(2, '0')}`;
        } else {
          this.endTime = '';
        }

        const c: Date = formValue.scheduledDate;
        formValue.weekday = c.toLocaleDateString('pt-PT', { weekday: 'long' });
      })
    );
  }

  onSubmit(): void {
    this.submitted = true;
    this.errorMessages = [];

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    this.loading = true;

    const formValue = this.form.value;

    // Convert time format (HH:MM) to decimal hours
    const convertTimeToDecimalHours = (timeString: string): number => {
      const [hours, minutes] = timeString.split(':').map(Number);
      return hours + minutes / 60;
    };

    // Create new session
    const createSession: CreateSession = {
      moduleTeachingId: formValue.moduleTeachingId,
      weekday: formValue.weekday,
      scheduledDate: formatDateForApi(formValue.scheduledDate),
      start: formValue.start,
      durationHours: convertTimeToDecimalHours(formValue.durationHours),
    };

    this.sessionsService.create(createSession).subscribe({
      next: (response) => {
        this.loading = false;
        this.bsModalRef.hide();
        this.sharedService.showSuccess('Sessão criada com sucesso.');
      },
      error: (error) => {
        this.errorMessages = this.sharedService.handleErrorResponse(error);
        this.loading = false;
      },
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
