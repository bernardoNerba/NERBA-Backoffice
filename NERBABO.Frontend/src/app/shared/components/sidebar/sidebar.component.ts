import { Component, HostListener, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SharedService } from '../../../core/services/shared.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
})
export class SidebarComponent implements OnInit {
  activePage: string = 'Dashboard';
  isAdmin!: boolean;

  mainMenuItems = [
    { name: 'Dashboard', icon: 'pi pi-chart-line', route: '/dashboard' },
    { name: 'Pessoas', icon: 'pi pi-users', route: '/people' },
    { name: 'Modulos', icon: 'pi pi-objects-column', route: '/modules' },
    { name: 'Cursos', icon: 'pi pi-graduation-cap', route: '/courses' },
    { name: 'Ações Formativas', icon: 'pi pi-bookmark-fill', route: '/actions' },
    { name: 'Empresas', icon: 'pi pi-building', route: '/companies' },
  ];

  adminMenuItems = [
    { name: 'Configurações', icon: 'pi pi-cog', route: '/config' },
    { name: 'Enquadramentos', icon: 'pi pi-expand', route: '/frames' },
    { name: 'Contas', icon: 'pi pi-users', route: '/accs' },
  ];

  bottomMenuItems = [
    {
      name: 'Notificações',
      icon: 'pi pi-bell',
      route: '/notifications',
    },
    {
      name: 'Logout',
      icon: 'pi pi-sign-out',
      route: '/logout',
    },
  ];

  constructor(
    private router: Router,
    private sharedService: SharedService,
    private authService: AuthService
  ) {
    this.router.events.subscribe(() => {
      const currentUrl = this.router.url;

      const allItems = [
        ...this.mainMenuItems,
        ...this.adminMenuItems,
        ...this.bottomMenuItems,
      ];

      const currentItem = allItems.find((item) =>
        currentUrl.startsWith(item.route)
      );

      if (currentItem) {
        this.activePage = currentItem.name;
      }
    });
  }

  ngOnInit(): void {
    this.isAdmin = this.authService.isUserAdmin;
    this.loadUserPreference();
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
}
