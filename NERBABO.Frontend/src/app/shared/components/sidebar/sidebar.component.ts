import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { SharedService } from '../../../core/services/shared.service';
import { AuthService } from '../../../core/services/auth.service';
import { MenuModule, Menu } from 'primeng/menu';
import { MenuItem } from 'primeng/api';

import { ButtonModule } from 'primeng/button';
import { Observable } from 'rxjs';
import { User } from '../../../core/models/user';
import { CommonModule } from '@angular/common';
import { TruncatePipe } from '../../pipes/truncate.pipe';

@Component({
  selector: 'app-sidebar',
  imports: [MenuModule, ButtonModule, CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
})
export class SidebarComponent implements OnInit {
  @ViewChild('menu') menu!: Menu;
  activePage: string = 'Dashboard';
  isAdmin!: boolean;
  profileMenuItems!: MenuItem[];
  user$?: Observable<User | null>;
  userRoles!: string[];
  displayRole!: string;
  userId!: string;

  mainMenuItems = [
    { name: 'Dashboard', icon: 'pi pi-chart-line', route: '/dashboard' },
    { name: 'Pessoas', icon: 'pi pi-users', route: '/people' },
    { name: 'Modulos', icon: 'pi pi-objects-column', route: '/modules' },
    { name: 'Cursos', icon: 'pi pi-graduation-cap', route: '/courses' },
    {
      name: 'Ações Formativas',
      icon: 'pi pi-bookmark-fill',
      route: '/actions',
    },
    { name: 'Empresas', icon: 'pi pi-building', route: '/companies' },
  ];

  adminMenuItems = [
    { name: 'Configurações', icon: 'pi pi-cog', route: '/config' },
    { name: 'Enquadramentos', icon: 'pi pi-expand', route: '/frames' },
    { name: 'Contas', icon: 'pi pi-users', route: '/accs' },
  ];

  constructor(
    private router: Router,
    private sharedService: SharedService,
    private authService: AuthService
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

    this.profileMenuItems = [
      {
        label: 'Notificações',
        icon: 'pi pi-bell',
        command: () => {
          this.router.navigate(['/notifications']);
        },
      },
      {
        label: 'Notificações',
        icon: 'pi pi-bell',
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
