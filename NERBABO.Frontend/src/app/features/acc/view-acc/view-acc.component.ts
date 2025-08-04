import { Component, OnInit, OnDestroy } from '@angular/core';
import { IView } from '../../../core/interfaces/IView';
import { ActivatedRoute, Router } from '@angular/router';
import { SharedService } from '../../../core/services/shared.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { MenuItem } from 'primeng/api';
import { catchError, Observable, of, Subscription, tap, forkJoin } from 'rxjs';
import { UserInfo } from '../../../core/models/userInfo';
import { ICONS } from '../../../core/objects/icons';
import { CommonModule } from '@angular/common';
import { Person } from '../../../core/models/person';
import { PeopleService } from '../../../core/services/people.service';
import { AccService } from '../../../core/services/acc.service';
import { NavHeaderComponent } from '../../../shared/components/nav-header/nav-header.component';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { ActionsTableComponent } from '../../../shared/components/tables/actions-table/actions-table.component';
import { Action } from '../../../core/models/action';
import { ActionsService } from '../../../core/services/actions.service';
import { IconComponent } from "../../../shared/components/icon/icon.component";

@Component({
  selector: 'app-view-acc',
  imports: [CommonModule, NavHeaderComponent, TitleComponent, ActionsTableComponent, IconComponent],
  templateUrl: './view-acc.component.html',
})
export class ViewAccComponent implements IView, OnInit, OnDestroy {
  user$?: Observable<UserInfo | null>;
  person$?: Observable<Person | null>;
  coordinatorActions$?: Observable<Action[]>;
  personId!: number; // person id from route
  userId!: string; // user id for coordinator actions
  fullName: string = '';
  menuItems: MenuItem[] | undefined;
  coordinatorActions: Action[] = [];
  actionsLoading: boolean = true;

  ICONS = ICONS;

  subscriptions: Subscription = new Subscription();

  constructor(
    private route: ActivatedRoute,
    private accService: AccService,
    private peopleService: PeopleService,
    private router: Router,
    private sharedService: SharedService,
    private bsModalService: BsModalService,
    private actionsService: ActionsService
  ) {}

  initializeEntity(): void {
    throw new Error('Method not implemented.');
  }
  onDeleteModal(): void {
    throw new Error('Method not implemented.');
  }

  ngOnInit(): void {
    const personId = this.route.snapshot.paramMap.get('id');
    this.personId = Number.parseInt(personId ?? '');

    if (isNaN(this.personId)) {
      this.router.navigate(['/people']);
      return;
    }

    this.initializePerson();
    this.initializeUser();
    this.populateMenu();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
  }

  initializePerson(): void {
    this.person$ = this.peopleService.getSinglePerson(this.personId).pipe(
      catchError((error) => {
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.router.navigate(['/people']);
          this.sharedService.showWarning('Informação não foi encontrada.');
        }
        return of(null);
      }),
      tap((person) => {
        if (person) {
          this.fullName = person.fullName;
          this.updateBreadcrumbs();
        } else {
          this.router.navigate(['/people', this.personId]);
          this.sharedService.showWarning('Pessoa não encontrada.');
        }
      })
    );
  }

  initializeUser(): void {
    // First, make sure we have users data loaded
    if (!this.accService.hasUsersData) {
      this.accService.triggerFetchUsers();
      // Wait for users to load, then try to find the user
      this.subscriptions.add(
        this.accService.users$.subscribe((users) => {
          if (users && users.length > 0) {
            this.findAndSetUser();
          }
        })
      );
    } else {
      this.findAndSetUser();
    }
  }

  private findAndSetUser(): void {
    const user = this.accService.getUserByPersonId(this.personId);
    if (user) {
      this.userId = user.id;
      this.user$ = of(user);
      this.initializeCoordinatorActions();
    } else {
      this.user$ = of(null);
      this.router.navigate(['/people', this.personId]);
      this.sharedService.showWarning('Utilizador não encontrado para esta pessoa.');
    }
  }

  initializeCoordinatorActions(): void {
    if (!this.userId) {
      this.actionsLoading = false;
      this.coordinatorActions$ = of([]);
      return;
    }

    this.actionsLoading = true;
    this.coordinatorActions$ = this.actionsService.getActionsByCoordinatorId(this.userId).pipe(
      catchError((error) => {
        console.warn('No actions found for coordinator:', error);
        this.actionsLoading = false;
        return of([]);
      }),
      tap((actions) => {
        this.coordinatorActions = actions;
        this.actionsLoading = false;
      })
    );
  }

  populateMenu(): void {
    const items: MenuItem[] = [
      {
        label: 'Editar',
        icon: 'pi pi-pencil',
        command: () => this.onUpdateModal(),
      },
      {
        label: 'Bloquear/Desbloquear',
        icon: 'pi pi-ban',
        command: () => this.onBlockModal(),
      },
    ];

    this.menuItems = [{ label: 'Opções', items }];
  }

  onUpdateModal(): void {
    // This would need to be implemented based on your UpsertAccComponent
    console.log('Update modal for user:', this.userId);
  }

  onBlockModal(): void {
    // This would need to be implemented based on your BlockAccComponent
    console.log('Block modal for user:', this.userId);
  }

  updateSourceSubscription(): void {
    this.subscriptions.add(
      this.accService.updatedSource$.subscribe((userId: string) => {
        if (this.userId === userId) {
          this.initializeUser();
          this.initializeCoordinatorActions();
        }
      })
    );
  }

  deleteSourceSubscription(): void {
    this.subscriptions.add(
      this.accService.deletedSource$.subscribe((deletedId: string) => {
        if (this.userId === deletedId) {
          this.router.navigateByUrl('/people/' + this.personId);
        }
      })
    );
  }

  updateBreadcrumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        displayName: 'Dashboard',
        url: '/dashboard',
        className: '',
      },
      {
        displayName: 'Pessoas',
        url: '/people',
        className: '',
      },
      {
        displayName:
          this.fullName.length > 21
            ? this.fullName.substring(0, 21) + '...'
            : this.fullName || 'Detalhes Pessoa',
        url: `/people/${this.personId}`,
        className: '',
      },
      {
        displayName: 'Detalhes Colaborador',
        url: `/people/${this.personId}/acc/`,
        className: 'inactive',
      },
    ]);
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}