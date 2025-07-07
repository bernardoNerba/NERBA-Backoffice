import { Component, Input, OnInit } from '@angular/core';
import { Module } from '../../../core/models/module';
import { BsModalRef } from 'ngx-bootstrap/modal';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ModulesService } from '../../../core/services/modules.service';
import { SharedService } from '../../../core/services/shared.service';
import { UniversalValidators } from 'ngx-validators';
import { OkResponse } from '../../../core/models/okResponse';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-update-modules',
  imports: [ErrorCardComponent, CommonModule, ReactiveFormsModule],
  templateUrl: './update-modules.component.html',
  styleUrl: './update-modules.component.css',
})
export class UpdateModulesComponent implements OnInit {
  errorMessages: string[] = [];
  @Input() id!: number;
  submitted: boolean = false;
  loading: boolean = false;
  form: FormGroup = new FormGroup({});

  currentModule!: Module;

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private modulesService: ModulesService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.initializeForm();

    this.loading = true;
    this.modulesService.getSingleModule(this.id).subscribe({
      next: (module: Module) => {
        this.currentModule = module;

        this.form.patchValue({
          name: this.currentModule.name,
          hours: this.currentModule.hours,
          isActive: this.currentModule.isActive,
        });
        this.loading = false;
      },
      error: (error) => {
        this.errorMessages = this.sharedService.handleErrorResponse(error);
        this.loading = false;
      },
    });
  }

  private initializeForm() {
    this.form = this.formBuilder.group({
      id: this.id,
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
    });
  }

  onSubmit() {
    this.submitted = true;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    const formValue = this.form.value;

    this.loading = true;
    this.modulesService.updateModule(this.id, formValue).subscribe({
      next: (value: OkResponse) => {
        this.bsModalRef.hide();
        this.modulesService.triggerFetch();
        this.sharedService.showSuccess(value.message);
        this.modulesService.notifyModuleUpdate(this.id);
      },
      error: (error) => {
        this.errorMessages = this.sharedService.handleErrorResponse(error);
        this.loading = false;
      },
    });
  }
}
