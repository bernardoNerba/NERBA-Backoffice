import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { NavigationLink } from '../../../core/models/navigationLink';
import { User } from '../../../core/models/user';
import { SharedService } from '../../../core/services/shared.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, CommonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent implements OnInit {
  breadcrumbs: Array<NavigationLink> = [];
  user$?: Observable<User | null>;

  constructor(
    private sharedService: SharedService,
    private authService: AuthService
  ) {}
  ngOnInit(): void {
    this.user$ = this.authService.user$;

    this.sharedService.breadcrumbList$.subscribe(
      (value) => (this.breadcrumbs = value)
    );
  }
  toggleSidebar(): void {
    this.sharedService.isCollapsed = !this.sharedService.isCollapsed;
  }

  get isCollapsed() {
    return this.sharedService.isCollapsed;
  }
}
