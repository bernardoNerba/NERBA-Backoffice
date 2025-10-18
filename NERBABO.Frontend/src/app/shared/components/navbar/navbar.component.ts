import { Component, HostListener, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { TooltipModule } from 'primeng/tooltip';
import { NavigationLink } from '../../../core/models/navigationLink';
import { User } from '../../../core/models/user';
import { SharedService } from '../../../core/services/shared.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, CommonModule, TooltipModule],
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

    // Store user preference in localStorage
    localStorage.setItem(
      'sidebarCollapsed',
      this.sharedService.isCollapsed.toString()
    );
  }

  get isCollapsed() {
    return this.sharedService.isCollapsed;
  }

  @HostListener('window:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent) {
    if (event.ctrlKey && event.key === 'b') {
      event.preventDefault();
      this.toggleSidebar();
    }
  }
}
