import { Component, Input, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { UserInfo } from '../../../core/models/userInfo';
import { SharedService } from '../../../core/services/shared.service';
import { AccService } from '../../../core/services/acc.service';
import { OkResponse } from '../../../core/models/okResponse';
import { PasswordValidators } from 'ngx-validators';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { Divider, DividerModule } from 'primeng/divider';
import { PasswordModule } from 'primeng/password';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-update-acc',
  imports: [
    ReactiveFormsModule,
    ErrorCardComponent,
    DividerModule,
    PasswordModule,
    AutoCompleteModule,
    InputTextModule,
  ],
  templateUrl: './update-acc.component.html',
  styles: `
  :host ::ng-deep .p-password {
  width: 100% !important;
  max-width: 100% !important;
  }
  :host ::ng-deep .p-password input {
    width: 100% !important;
  }
  `,
})
export class UpdateAccComponent implements OnInit {
  @Input() id!: string;
  currentUser!: UserInfo;
  updateUserForm: FormGroup = new FormGroup({});
  submitted: boolean = false;
  errorMessages: string[] = [];
  loading = false;

  constructor(
    public bsModalRef: BsModalRef,
    private userService: AccService,
    private formBuilder: FormBuilder,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    const userFromService = this.userService.getuserById(this.id);
    if (userFromService) this.currentUser = userFromService;

    this.initializeForm();
  }

  private initializeForm() {
    this.updateUserForm = this.formBuilder.group(
      {
        email: [
          this.currentUser.email,
          [
            Validators.required,
            Validators.maxLength(50),
            Validators.minLength(8),
            Validators.email,
          ],
        ],
        userName: [
          this.currentUser.userName,
          [
            Validators.required,
            Validators.minLength(3),
            Validators.maxLength(30),
          ],
        ],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            Validators.maxLength(30),
          ],
        ],
        confirmPassword: ['', [Validators.required]],
      },
      {
        validators: PasswordValidators.mismatchedPasswords(
          'password',
          'confirmPassword'
        ),
      }
    );
  }

  onSubmit() {
    this.submitted = true;
    this.errorMessages = [];

    if (this.updateUserForm.invalid) {
      this.updateUserForm.markAllAsTouched();
      this.sharedService.showError(
        'Os dados fornecidos não estão de acordo com as diretrizes.'
      );
      return;
    }

    this.loading = true;
    this.userService
      .updateUser({
        id: this.id,
        userName: this.updateUserForm.get('userName')?.value,
        email: this.updateUserForm.get('email')?.value,
        newPassword: this.updateUserForm.get('password')?.value,
      })
      .subscribe({
        next: (response: OkResponse) => {
          console.log(response);
          this.sharedService.showSuccess(response.title);
          this.bsModalRef.hide();
          this.userService.triggerFetchUsers();
        },
        error: (err) => {
          this.sharedService.handleErrorResponse(err);
          this.loading = false;
        },
      });
  }
}
