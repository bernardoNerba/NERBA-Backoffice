import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { UniversalValidators } from 'ngx-validators';
import { Observable, Subject, takeUntil } from 'rxjs';
import { ErrorCardComponent } from '../../shared/components/error-card/error-card.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { Tax } from '../../core/models/tax';
import { GeneralInfo } from '../../core/models/generalInfo';
import { SharedService } from '../../core/services/shared.service';
import { ConfigService } from '../../core/services/config.service';
import { OkResponse } from '../../core/models/okResponse';
import { IndexTaxesComponent } from './taxes/index-taxes/index-taxes.component';

@Component({
  selector: 'app-global-config',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ErrorCardComponent,
    SpinnerComponent,
    IndexTaxesComponent,
  ],
  templateUrl: './global-config.component.html',
  styleUrl: './global-config.component.css',
})
export class GlobalConfigComponent implements OnInit, OnDestroy {
  formGeneralInfo: FormGroup = new FormGroup({});
  submittedGeneralInfo = false;
  loadingGeneralInfo = false;
  errorMessagesGeneralInfo = [];
  private destroy$ = new Subject<void>();
  configurationInfo$!: Observable<GeneralInfo | null>;
  taxes$!: Observable<Array<Tax>>;
  loading$!: Observable<boolean>;

  constructor(
    private fb: FormBuilder,
    private sharedService: SharedService,
    private confService: ConfigService
  ) {}
  ngOnInit(): void {
    this.loadConfiguration();
    this.initializeForm();
    this.updateBreadcrumbs();
  }

  private loadConfiguration(): void {
    this.taxes$ = this.confService.taxes$;
    this.configurationInfo$ = this.confService.configurationInfo$;
    this.loading$ = this.confService.loading$;

    this.configurationInfo$.pipe(takeUntil(this.destroy$)).subscribe({
      next: (config) => {
        console.log('Configuration data:', config);
        if (config) {
          this.formGeneralInfo.patchValue({
            designation: config.designation ?? '',
            site: config.site ?? '',
            ivaId: config.ivaId ?? '',
            hourValueTeacher: config.hourValueTeacher ?? '',
            hourValueAlimentation: config.hourValueAlimentation ?? '',
            bankEntity: config.bankEntity ?? '',
            iban: config.iban ?? '',
            nipc: config.nipc ?? '',
          });
        }
      },
      error: (err) => {
        console.error('Error loading configuration:', err);
        this.sharedService.showError('Failed to load configuration');
      },
    });
  }

  private initializeForm(): void {
    this.formGeneralInfo = this.fb.group({
      designation: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(255),
        ],
      ],
      site: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(500),
        ],
      ],
      ivaId: ['', [Validators.required]],
      hourValueTeacher: [
        '',
        [Validators.required, UniversalValidators.isNumber],
      ],
      hourValueAlimentation: [
        '',
        [Validators.required, UniversalValidators.isNumber],
      ],
      bankEntity: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(50),
        ],
      ],
      iban: [
        '',
        [
          Validators.required,
          Validators.minLength(25),
          Validators.maxLength(25),
        ],
      ],
      nipc: [
        '',
        [
          Validators.required,
          Validators.minLength(9),
          Validators.maxLength(9),
          UniversalValidators.isNumber,
        ],
      ],
      logoFinancing: [''],
    });
  }

  onSubmitGeneralInfo() {
    this.submittedGeneralInfo = true;
    this.errorMessagesGeneralInfo = [];

    if (this.formGeneralInfo.invalid) {
      this.formGeneralInfo.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    this.loadingGeneralInfo = true;

    const model = this.formGeneralInfo.value;

    this.confService.updateGeneralInfo(model).subscribe({
      next: (value: OkResponse) => {
        this.confService.triggerFetchConfigs();
        this.sharedService.showSuccess(value.message);
        this.loadingGeneralInfo = false;
      },
      error: (error: any) => {
        this.sharedService.handleErrorResponse(error);
      },
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/config',
        displayName: 'Configurações',
        className: 'inactive',
      },
    ]);
  }
}
