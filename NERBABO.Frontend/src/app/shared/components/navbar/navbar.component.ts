import { Component, HostListener, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { TooltipModule } from 'primeng/tooltip';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { NavigationLink } from '../../../core/models/navigationLink';
import { User } from '../../../core/models/user';
import { SharedService } from '../../../core/services/shared.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, CommonModule, TooltipModule, BadgeModule, ButtonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent implements OnInit, OnDestroy {
  breadcrumbs: Array<NavigationLink> = [];
  user$?: Observable<User | null>;
  notificationCount = 0;
  private destroy$ = new Subject<void>();

  constructor(
    private sharedService: SharedService,
    private authService: AuthService,
    public router: Router,
    private notificationService: NotificationService
  ) {}
  ngOnInit(): void {
    this.user$ = this.authService.user$;

    this.sharedService.breadcrumbList$.subscribe(
      (value) => (this.breadcrumbs = value)
    );

    this.notificationService.notificationCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe((count) => {
        this.notificationCount = count.unreadCount;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
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

  logout(): void {
    this.router.navigate(['/logout']);
  }
}
