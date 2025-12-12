import { Component, HostListener, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { SharedService } from '../../../core/services/shared.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { MenuModule, Menu } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { BadgeModule } from 'primeng/badge';

import { ButtonModule } from 'primeng/button';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { User } from '../../../core/models/user';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-sidebar',
  imports: [MenuModule, ButtonModule, CommonModule, BadgeModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
})
export class SidebarComponent implements OnInit, OnDestroy {
  @ViewChild('menu') menu!: Menu;
  activePage: string = 'Dashboard';
  isAdmin!: boolean;
  profileMenuItems!: MenuItem[];
  user$?: Observable<User | null>;
  userRoles!: string[];
  displayRole!: string;
  userId!: string;
  notificationCount = 0;
  private destroy$ = new Subject<void>();

  mainMenuItems = [
    {
      name: 'Dashboard',
      icon: 'pi pi-chart-line',
      route: '/dashboard',
      admin: false,
    },
    { name: 'Pessoas', icon: 'pi pi-users', route: '/people', admin: false },
    {
      name: 'Modulos',
      icon: 'pi pi-objects-column',
      route: '/modules',
      admin: false,
    },
    {
      name: 'Cursos',
      icon: 'pi pi-graduation-cap',
      route: '/courses',
      admin: false,
    },
    {
      name: 'Ações Formativas',
      icon: 'pi pi-bookmark-fill',
      route: '/actions',
      admin: false,
    },
    {
      name: 'Empresas',
      icon: 'pi pi-building',
      route: '/companies',
      admin: false,
    },
    {
      name: 'Enquadramentos',
      icon: 'pi pi-expand',
      route: '/frames',
      admin: false,
    },
    { name: 'Contas', icon: 'pi pi-users', route: '/accs', admin: false },
    {
      name: 'Configurações',
      icon: 'pi pi-cog',
      route: '/config',
      admin: true,
    },
  ];

  adminMenuItems = [];

  constructor(
    private router: Router,
    private sharedService: SharedService,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {
    this.router.events.subscribe(() => {
      const currentUrl = this.router.url;

      const allItems = [...this.mainMenuItems, ...this.adminMenuItems];

      const currentItem = allItems.find((item) =>
        currentUrl.startsWith(item.route)
      );

      if (currentItem) {
        this.activePage = currentItem.name;
      }
    });

    this.user$ = this.authService.user$;
    this.userRoles = this.authService.userRoles;
  }

  ngOnInit(): void {
    this.isAdmin = this.authService.isUserAdmin;
    this.loadUserPreference();
    this.setDisplayRole();

    // Subscribe to notification count
    this.notificationService.notificationCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe((count) => {
        this.notificationCount = count.unreadCount;
        this.updateProfileMenuItems();
      });

    this.updateProfileMenuItems();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateProfileMenuItems(): void {
    this.profileMenuItems = [
      {
        label: 'Notificações',
        icon: 'pi pi-bell',
        badge: this.notificationCount > 0 ? this.notificationCount.toString() : undefined,
        command: () => {
          this.router.navigate(['/notifications']);
        },
      },
      {
        label: 'Logout',
        icon: 'pi pi-sign-out',
        command: () => {
          this.router.navigate(['/logout']);
        },
      },
    ];
  }

  setActivePage(route: string, pageName: string): void {
    this.activePage = pageName;
    this.router.navigate([route]);
  }

  get isCollapsed(): boolean {
    return this.sharedService.isCollapsed;
  }

  // Load user preference on component init
  private loadUserPreference(): void {
    const savedState = localStorage.getItem('sidebarCollapsed');
    if (savedState !== null) {
      this.sharedService.isCollapsed = savedState === 'true';
    }
  }

  @HostListener('window:resize', [])
  onResize() {
    // Only auto-collapse on very small screens if user hasn't manually set preference
    this.checkScreenSize();
  }

  toggleMenu(event: Event) {
    if (this.isCollapsed) {
      this.menu.toggle(event);
    }
  }

  private checkScreenSize() {
    // Only auto-collapse on very small screens (mobile phones)
    // Never auto-expand - only auto-collapse when screen is very small
    const isVerySmallScreen = window.innerWidth < 576;

    if (isVerySmallScreen) {
      this.sharedService.isCollapsed = true;
    }
  }

  setDisplayRole(): void {
    const roles = this.userRoles;

    if (roles.includes('Admin')) {
      this.displayRole = 'Admin';
      return;
    }

    const isFM = roles.includes('FM');
    const isCQ = roles.includes('CQ');

    if (isFM && isCQ) {
      this.displayRole = 'FM / CQ';
    } else if (isFM) {
      this.displayRole = 'FM';
    } else if (isCQ) {
      this.displayRole = 'CQ';
    } else if (roles.includes('User')) {
      this.displayRole = 'User';
    }
  }
}
