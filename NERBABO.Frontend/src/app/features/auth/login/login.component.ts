import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { take } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { SharedService } from '../../../core/services/shared.service';
import { User } from '../../../core/models/user';
import { Login } from '../../../core/models/login';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    InputTextModule,
    PasswordModule,
    ButtonModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup = new FormGroup({}); // form reference
  submitted: boolean = false; // form was submitted?
  loading: boolean = false; // loading state for button
  errorMessages: string[] = []; // Array of erros
  returnUrl: string | null = null; // url from there the user was redirected to login page

  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private sharedService: SharedService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    // track return Url
    this.authService.user$.pipe(take(1)).subscribe({
      next: (user: User | null) => {
        if (user) {
          // user is already authenticated
          this.router.navigateByUrl('/');
        } else {
          // in case that is no user subscribe to the built in
          // activated route and query the url params looking for 'returnUrl'
          this.activatedRoute.queryParamMap.subscribe({
            next: (params: any) => {
              if (params) {
                this.returnUrl = params.get('returnUrl');
              }
            },
          });
        }
      },
    });
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.loginForm = this.formBuilder.group({
      usernameOrEmail: ['', [Validators.required]],
      password: ['', [Validators.required]],
    });
  }

  login() {
    this.submitted = true;
    this.errorMessages = [];

    if (this.loginForm.valid) {
      this.loading = true;
      // after checking validatores try login
      this.authService.login(this.loginForm.value as Login).subscribe({
        next: () => {
          if (this.returnUrl) {
            // case there is a return url navigate there
            this.router.navigateByUrl(this.returnUrl);
          } else {
            // no url navigate to dashboard
            this.router.navigateByUrl('/dashboard');
          }
          // display welcome message
          this.sharedService.showSuccess('Bem-Vindo(a) ao Backoffice.');
          this.loading = false;
        },
        error: (error) => {
          // display error from api
          this.sharedService.handleErrorResponse(error);
          this.loading = false;
        },
      });
    }
  }
}
