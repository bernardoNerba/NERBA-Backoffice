import { Component, Input, OnInit } from '@angular/core';
import { IUpsert } from '../../../core/interfaces/IUpsert';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ModulesService } from '../../../core/services/modules.service';
import { SharedService } from '../../../core/services/shared.service';
import { UniversalValidators } from 'ngx-validators';
import { Module } from '../../../core/models/module';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { InputNumber } from 'primeng/inputnumber';
import { CommonModule } from '@angular/common';
import { InputTextModule } from 'primeng/inputtext';
import { ConfigService } from '../../../core/services/config.service';
import { ModuleCategory } from '../../global-config/module-categories/module-category.model';
import { SelectModule } from 'primeng/select';

@Component({
  selector: 'app-upsert-modules',
  imports: [
    ErrorCardComponent,
    InputNumber,
    ReactiveFormsModule,
    CommonModule,
    InputTextModule,
    SelectModule
  ],
  templateUrl: './upsert-modules.component.html',
})
export class UpsertModulesComponent implements IUpsert, OnInit {
  @Input({ required: true }) id!: number;
  currentModule?: Module | null;

  submitted: boolean = false;
  loading: boolean = false;
  isUpdate: boolean = false;
  categories?: {id:number, name:string}[];
  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});


  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private moduleService: ModulesService,
    private sharedService: SharedService,
    private confService: ConfigService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    if (this.id !== 0) {
      this.isUpdate = true;
      this.moduleService.getSingleModule(this.id).subscribe({
        next: (module: Module) => {
          this.currentModule = module;
          this.patchFormValues();
        },
        error: () => {
          this.sharedService.showError('Módulo não encontrado.');
          this.bsModalRef.hide();
        },
      });
    } else {
      this.initializeCategories();
      console.log(this.categories)
    }
  }

  initializeForm(): void {
    this.form = this.formBuilder.group({
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(255),
        ],
      ],
      hours: [
        '',
        [Validators.required, UniversalValidators.isInRange(0, 1000)],
      ],
      isActive: [true],
      category: ''
    });
  }



  private initializeCategories(): void {
  this.confService.moduleCategories$.subscribe({
    next: (categories: ModuleCategory[]) => {
      this.categories = [... categories.map(c => ({
        id: c.id,
        name: `${c.shortenName} - ${c.name}`
      }))];
    },
    error: (error: any) => {
      console.error("Error loading categories:", error);
      this.bsModalRef.hide();
    }
  });
}

  patchFormValues(): void {
    this.form.patchValue({
      name: this.currentModule?.name,
      hours: this.currentModule?.hours,
      isActive: this.currentModule?.isActive,
    });
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

    console.log(this.form.value);

    this.loading = true;

    this.moduleService
      .upsertModule({ id: this.id, ...this.form.value }, this.isUpdate)
      .subscribe({
        next: (value) => {
          this.bsModalRef.hide();
          this.moduleService.triggerFetch();
          this.sharedService.showSuccess(value.message);
        },
        error: (error) => {
          console.log(error.error)
          this.errorMessages = this.sharedService.handleErrorResponse(error);
          this.loading = false;
        },
      });
  }
}
