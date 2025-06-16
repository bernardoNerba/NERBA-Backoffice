import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { SharedService } from '../../../core/services/shared.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-logout',
  imports: [],
  template: `<div class="text-center text-success">A fazer logout...</div>`,
})
export class LogoutComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private sharedService: SharedService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.logout();
  }

  private logout(): void {
    this.authService.logout();
    this.sharedService.showSuccess('Logout efetuado com sucesso.');
    this.router.navigateByUrl('/login');
  }
}
