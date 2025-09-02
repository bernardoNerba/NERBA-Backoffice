import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { Observable, of, Subscription } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { PeopleService } from '../../../core/services/people.service';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Person } from '../../../core/models/person';
import { SharedService } from '../../../core/services/shared.service';
import { DeletePeopleComponent } from '../delete-people/delete-people.component';
import { ICONS } from '../../../core/objects/icons';
import { MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { IView } from '../../../core/interfaces/IView';
import { UpsertPeopleComponent } from '../upsert-people/upsert-people.component';
import { UpsertTeachersComponent } from '../../teachers/upsert-teachers/upsert-teachers.component';
import { UpsertAccComponent } from '../../acc/upsert-acc/upsert-acc.component';
import { UpsertStudentsComponent } from '../../students/upsert-students/upsert-students.component';
import { NavHeaderComponent } from '../../../shared/components/nav-header/nav-header.component';
import { TeachersService } from '../../../core/services/teachers.service';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { StudentsService } from '../../../core/services/students.service';
import { AccService } from '../../../core/services/acc.service';
import { PdfFileManagerComponent, PdfFileConfig } from '../../../shared/components/pdf-file-manager/pdf-file-manager.component';

@Component({
  selector: 'app-detail-person',
  standalone: true,
  imports: [CommonModule, RouterModule, NavHeaderComponent, TitleComponent, TooltipModule, PdfFileManagerComponent],
  templateUrl: './view-people.component.html',
})
export class ViewPeopleComponent implements IView, OnInit, OnDestroy {
  person$?: Observable<Person | null>;
  selectedId!: number;
  fullName!: string;
  id!: number;
  personId!: number;
  ICONS = ICONS;
  menuItems: MenuItem[] | undefined;

  isStudent: boolean = false;
  isTeacher: boolean = false;
  isColaborator: boolean = false;

  // PDF file manager configurations
  habilitationPdfConfig: PdfFileConfig = {
    title: 'Certificado de Habilitações',
    description: 'Nenhum certificado de habilitações foi carregado.',
    icon: 'bi bi-file-earmark-pdf',
    fileNamePrefix: 'Certificado_de_Habilitacoes'
  };

  nifPdfConfig: PdfFileConfig = {
    title: 'Comprovativo de NIF',
    description: 'Nenhum comprovativo de NIF foi carregado.',
    icon: 'bi bi-file-earmark-pdf',
    fileNamePrefix: 'Comprovativo_NIF'
  };

  identificationDocumentPdfConfig: PdfFileConfig = {
    title: 'Cópia do Documento de Identificação',
    description: 'Nenhuma cópia do documento de identificação foi carregada.',
    icon: 'bi bi-file-earmark-pdf',
    fileNamePrefix: 'Documento_Identificacao'
  };

  // Loading states
  isUploadingHabilitation: boolean = false;
  isUploadingNif: boolean = false;
  isUploadingIdentification: boolean = false;

  subscriptions: Subscription = new Subscription();

  constructor(
    private route: ActivatedRoute,
    private peopleService: PeopleService,
    private teacherService: TeachersService,
    private studentsService: StudentsService,
    private accService: AccService,
    private router: Router,
    private sharedService: SharedService,
    private bsModalService: BsModalService
  ) {}

  ngOnInit(): void {
    const personId = this.route.snapshot.paramMap.get('id');
    this.id = Number.parseInt(personId ?? '');

    if (isNaN(this.id)) {
      this.router.navigate(['/people']);
      return;
    }

    this.initializeEntity();
    this.updateSourceSubscription();
    this.deleteSourceSubscription();
    this.updateSourceProfilesSubscription();
    this.deleteSourceProfilesSubscription();
  }

  onUpdateModal() {
    const initialState = {
      id: this.id,
    };
    this.bsModalService.show(UpsertPeopleComponent, {
      initialState: initialState,
      class: 'modal-lg',
    });
  }

  onDeleteModal() {
    const initialState = {
      id: this.id,
      fullName: this.fullName,
    };
    this.bsModalService.show(DeletePeopleComponent, { initialState });
  }

  onCreateColaborator() {
    this.bsModalService.show(UpsertAccComponent, {
      initialState: {
        id: 0,
        personId: this.id,
      },
      class: 'modal-md',
    });
  }

  onCreateStudent() {
    this.bsModalService.show(UpsertStudentsComponent, {
      initialState: {
        id: 0,
        personId: this.id,
      },
      class: 'modal-lg',
    });
  }

  onCreateTeacher() {
    this.bsModalService.show(UpsertTeachersComponent, {
      initialState: {
        id: 0,
        personId: this.id,
      },
      class: 'modal-lg',
    });
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
            : this.fullName || 'Detalhes',
        url: `/people/${this.id}`,
        className: 'inactive',
      },
    ]);
  }

  initializeEntity() {
    this.person$ = this.peopleService.getSinglePerson(this.id).pipe(
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
          this.id = person.id;
          this.personId = person.id;
          this.isColaborator = person.isColaborator ?? false;
          this.isStudent = person.isStudent ?? false;
          this.isTeacher = person.isTeacher ?? false;
          this.updateBreadcrumbs();
          this.populateMenu();
        }
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
        label: 'Eliminar',
        icon: 'pi pi-exclamation-triangle',
        command: () => this.onDeleteModal(),
      },
    ];

    if (!this.isTeacher) {
      items.push({
        label: 'Criar como Formador',
        icon: 'pi pi-plus',
        command: () => this.onCreateTeacher(),
      });
    }

    if (!this.isStudent) {
      items.push({
        label: 'Criar como Formando',
        icon: 'pi pi-plus',
        command: () => this.onCreateStudent(),
      });
    }

    if (!this.isColaborator) {
      items.push({
        label: 'Criar como Colaborador',
        icon: 'pi pi-plus',
        command: () => this.onCreateColaborator(),
      });
    }

    this.menuItems = [
      {
        label: 'Opções',
        items,
      },
    ];
  }

  updateSourceSubscription() {
    this.subscriptions.add(
      this.peopleService.updatedSource$.subscribe((updatedId: number) => {
        if (this.id === updatedId) {
          this.initializeEntity();
        }
      })
    );
  }

  deleteSourceSubscription() {
    this.subscriptions.add(
      this.peopleService.deletedSource$.subscribe((deletedId: number) => {
        if (this.id === deletedId) {
          this.router.navigateByUrl('/people');
        }
      })
    );
  }

  deleteSourceProfilesSubscription() {
    this.subscriptions.add(
      this.teacherService.deletedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
        this.initializeEntity();
      })
    );
    this.subscriptions.add(
      this.studentsService.deletedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
        this.initializeEntity();
      })
    );
    this.subscriptions.add(
      this.accService.deletedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
        this.initializeEntity();
      })
    );
  }

  updateSourceProfilesSubscription() {
    this.subscriptions.add(
      this.teacherService.updatedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
        this.initializeEntity();
      })
    );
    this.subscriptions.add(
      this.studentsService.updatedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
        this.initializeEntity();
      })
    );
    this.subscriptions.add(
      this.accService.updatedSource$.subscribe(() => {
        this.peopleService.triggerFetchPeople();
        this.initializeEntity();
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  // PDF handling methods - Habilitation
  private habilitationSelectedFile: File | null = null;

  onHabilitationFileSelected(file: File): void {
    this.habilitationSelectedFile = file;
  }

  uploadHabilitationPdf(): void {
    if (!this.habilitationSelectedFile) {
      this.sharedService.showWarning('Por favor selecione um ficheiro PDF.');
      return;
    }

    this.isUploadingHabilitation = true;
    
    this.peopleService.uploadHabilitationPdf(this.id, this.habilitationSelectedFile).subscribe({
      next: (response) => {
        this.sharedService.showSuccess('PDF carregado com sucesso.');
        this.habilitationSelectedFile = null;
        this.initializeEntity();
      },
      error: (error) => {
        console.error('Error uploading habilitation PDF:', error);
        if (error.status === 401 || error.status === 403) {
          this.sharedService.redirectUser();
        } else {
          this.sharedService.showError('Erro ao carregar o PDF. Tente novamente.');
        }
      },
      complete: () => {
        this.isUploadingHabilitation = false;
      }
    });
  }

  downloadHabilitationPdf(): void {
    this.peopleService.downloadHabilitationPdf(this.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Certificado_Habilitacoes_${this.fullName.replace(/\s+/g, '_')}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.sharedService.showSuccess('PDF descarregado com sucesso.');
      },
      error: (error) => {
        this.handlePdfError(error, 'descarregar');
      }
    });
  }

  viewHabilitationPdf(): void {
    this.peopleService.downloadHabilitationPdf(this.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => window.URL.revokeObjectURL(url), 1000);
      },
      error: (error) => {
        this.handlePdfError(error, 'visualizar');
      }
    });
  }

  deleteHabilitationPdf(): void {
    if (!confirm('Tem a certeza que deseja eliminar este PDF? Esta ação não pode ser desfeita.')) {
      return;
    }

    this.peopleService.deleteHabilitationPdf(this.id).subscribe({
      next: (response) => {
        this.sharedService.showSuccess('PDF eliminado com sucesso.');
        this.initializeEntity();
      },
      error: (error) => {
        this.handlePdfError(error, 'eliminar');
      }
    });
  }

  // NIF PDF handling methods
  private nifSelectedFile: File | null = null;

  onNifFileSelected(file: File): void {
    this.nifSelectedFile = file;
  }

  uploadNifPdf(): void {
    if (!this.nifSelectedFile) {
      this.sharedService.showWarning('Por favor selecione um ficheiro PDF.');
      return;
    }

    this.isUploadingNif = true;
    
    this.peopleService.uploadNifPdf(this.id, this.nifSelectedFile).subscribe({
      next: (response) => {
        this.sharedService.showSuccess('PDF carregado com sucesso.');
        this.nifSelectedFile = null;
        this.initializeEntity();
      },
      error: (error) => {
        this.handlePdfError(error, 'carregar');
      },
      complete: () => {
        this.isUploadingNif = false;
      }
    });
  }

  downloadNifPdf(): void {
    this.peopleService.downloadNifPdf(this.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Comprovativo_NIF_${this.fullName.replace(/\s+/g, '_')}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.sharedService.showSuccess('PDF descarregado com sucesso.');
      },
      error: (error) => {
        this.handlePdfError(error, 'descarregar');
      }
    });
  }

  viewNifPdf(): void {
    this.peopleService.downloadNifPdf(this.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => window.URL.revokeObjectURL(url), 1000);
      },
      error: (error) => {
        this.handlePdfError(error, 'visualizar');
      }
    });
  }

  deleteNifPdf(): void {
    if (!confirm('Tem a certeza que deseja eliminar este PDF? Esta ação não pode ser desfeita.')) {
      return;
    }

    this.peopleService.deleteNifPdf(this.id).subscribe({
      next: (response) => {
        this.sharedService.showSuccess('PDF eliminado com sucesso.');
        this.initializeEntity();
      },
      error: (error) => {
        this.handlePdfError(error, 'eliminar');
      }
    });
  }

  // Identification Document PDF handling methods
  private identificationDocumentSelectedFile: File | null = null;

  onIdentificationDocumentFileSelected(file: File): void {
    this.identificationDocumentSelectedFile = file;
  }

  uploadIdentificationDocumentPdf(): void {
    if (!this.identificationDocumentSelectedFile) {
      this.sharedService.showWarning('Por favor selecione um ficheiro PDF.');
      return;
    }

    this.isUploadingIdentification = true;
    
    this.peopleService.uploadIdentificationDocumentPdf(this.id, this.identificationDocumentSelectedFile).subscribe({
      next: (response) => {
        this.sharedService.showSuccess('PDF carregado com sucesso.');
        this.identificationDocumentSelectedFile = null;
        this.initializeEntity();
      },
      error: (error) => {
        this.handlePdfError(error, 'carregar');
      },
      complete: () => {
        this.isUploadingIdentification = false;
      }
    });
  }

  downloadIdentificationDocumentPdf(): void {
    this.peopleService.downloadIdentificationDocumentPdf(this.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Documento_Identificacao_${this.fullName.replace(/\s+/g, '_')}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.sharedService.showSuccess('PDF descarregado com sucesso.');
      },
      error: (error) => {
        this.handlePdfError(error, 'descarregar');
      }
    });
  }

  viewIdentificationDocumentPdf(): void {
    this.peopleService.downloadIdentificationDocumentPdf(this.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => window.URL.revokeObjectURL(url), 1000);
      },
      error: (error) => {
        this.handlePdfError(error, 'visualizar');
      }
    });
  }

  deleteIdentificationDocumentPdf(): void {
    if (!confirm('Tem a certeza que deseja eliminar este PDF? Esta ação não pode ser desfeita.')) {
      return;
    }

    this.peopleService.deleteIdentificationDocumentPdf(this.id).subscribe({
      next: (response) => {
        this.sharedService.showSuccess('PDF eliminado com sucesso.');
        this.initializeEntity();
      },
      error: (error) => {
        this.handlePdfError(error, 'eliminar');
      }
    });
  }

  // Helper method for PDF error handling
  private handlePdfError(error: any, action: string): void {
    console.error(`Error ${action} PDF:`, error);
    if (error.status === 401 || error.status === 403) {
      this.sharedService.redirectUser();
    } else if (error.status === 404) {
      this.sharedService.showWarning('PDF não encontrado.');
    } else {
      this.sharedService.showError(`Erro ao ${action} o PDF. Tente novamente.`);
    }
  }
}
