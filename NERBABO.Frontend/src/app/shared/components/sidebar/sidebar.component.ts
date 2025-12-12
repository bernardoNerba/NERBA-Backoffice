import { Component, HostListener, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SharedService } from '../../../core/services/shared.service';
import { AuthService } from '../../../core/services/auth.service';
import { Observable } from 'rxjs';
import { User } from '../../../core/models/user';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-sidebar',
  imports: [CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
})
export class SidebarComponent implements OnInit {
  activePage: string = 'Dashboard';
  isAdmin!: boolean;
  user$?: Observable<User | null>;
  userRoles!: string[];
  displayRole!: string;
  userId!: string;

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
    { name: 'Contas', icon: 'pi pi-id-card', route: '/accs', admin: false },
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
