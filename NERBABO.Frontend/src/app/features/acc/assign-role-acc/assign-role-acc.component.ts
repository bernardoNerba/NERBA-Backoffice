import { Component, Input, OnInit } from '@angular/core';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';
import { environment } from '../../../../environments/environment.development';
import { MultiSelectModule } from 'primeng/multiselect';
import { UserInfo } from '../../../core/models/userInfo';
import { AccService } from '../../../core/services/acc.service';
import { SharedService } from '../../../core/services/shared.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-assign-role-acc',
  imports: [
    TypeaheadModule,
    ReactiveFormsModule,
    MultiSelectModule,
    FormsModule,
  ],
  templateUrl: './assign-role-acc.component.html',
  styleUrl: './assign-role-acc.component.css',
})
export class AssignRoleAccComponent implements OnInit {
  @Input() user!: UserInfo;

  form: FormGroup = new FormGroup({
    roleInput: new FormControl('', Validators.required),
  });

  availableRoles: string[] = `${environment.roles}`.trim().split(',');
  selectedRoles: string[] = [];

  constructor(
    public bsModalRef: BsModalRef,
    private accService: AccService,
    private sharedService: SharedService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.form.get('roleInput')?.valueChanges.subscribe((value) => {
      console.log(value);
    });

    if (Array.isArray(this.user.roles)) {
      this.user.roles.forEach((element) => {
        this.selectedRoles.push(element);
      });
    }
  }

  onSelectRole(selected: string): void {
    if (!this.selectedRoles.includes(selected)) {
      this.selectedRoles.push(selected);
    }
    this.form.get('roleInput')?.reset(); // Clear input
  }

  removeRole(role: string): void {
    this.selectedRoles = this.selectedRoles.filter((r) => r !== role);
  }

  get filteredRoles(): string[] {
    return this.availableRoles.filter(
      (role) =>
        !this.selectedRoles.includes(role) && !this.user.roles.includes(role)
    );
  }

  onSubmit() {
    console.log('clicked');
    this.authService
      .assignRole({
        userId: this.user.id,
        roles: this.selectedRoles,
      })
      .subscribe({
        next: (value) => {
          this.sharedService.showSuccess(value.message);
          this.bsModalRef.hide();
          this.accService.triggerFetchUsers();
        },
        error: (error) => {
          this.sharedService.handleErrorResponse(error);
        },
      });
  }
}
