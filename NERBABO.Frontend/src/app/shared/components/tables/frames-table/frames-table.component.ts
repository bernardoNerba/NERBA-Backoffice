import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Table, TableModule } from 'primeng/table';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Button } from 'primeng/button';
import { Menu } from 'primeng/menu';
import { Frame } from '../../../../core/models/frame';
import { MenuItem } from 'primeng/api';
import { Subscription } from 'rxjs';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Router } from '@angular/router';
import { FrameService } from '../../../../core/services/frame.service';
import { DeleteFramesComponent } from '../../../../features/frames/delete-frames/delete-frames.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TruncatePipe } from '../../../pipes/truncate.pipe';
import { SpinnerComponent } from '../../spinner/spinner.component';
import { InputTextModule } from 'primeng/inputtext';
import { IconAnchorComponent } from '../../anchors/icon-anchor.component';
import { UpsertFramesComponent } from '../../../../features/frames/upsert-frames/upsert-frames.component';
import { SharedService } from '../../../../core/services/shared.service';

@Component({
  selector: 'app-frames-table',
  imports: [
    TableModule,
    IconField,
    InputIcon,
    Button,
    Menu,
    CommonModule,
    FormsModule,
    TruncatePipe,
    SpinnerComponent,
    InputTextModule,
    IconAnchorComponent,
  ],
  templateUrl: './frames-table.component.html',
})
export class FramesTableComponent implements OnInit {
  @Input({ required: true }) frames!: Frame[];
  @Input({ required: true }) loading!: boolean;
  @ViewChild('dt') dt!: Table;
  menuItems: MenuItem[] | undefined;
  searchValue: string = '';
  selectedFrame: Frame | undefined;
  first = 0;
  rows = 10;

  private subscriptions = new Subscription();

  constructor(
    private modalService: BsModalService,
    private router: Router,
    private framesService: FrameService,
    private sharedService: SharedService
  ) {}

  ngOnInit(): void {
    this.menuItems = [
      {
        label: 'Opções',
        items: [
          {
            label: 'Editar',
            icon: 'pi pi-pencil',
            command: () => this.onUpdateFrameModal(this.selectedFrame!),
          },
          {
            label: 'Eliminar',
            icon: 'pi pi-exclamation-triangle',
            command: () => this.onDeleteFrameModal(this.selectedFrame!),
          },
          {
            label: 'Detalhes',
            icon: 'pi pi-exclamation-circle',
            command: () =>
              this.router.navigateByUrl(`/frames/${this.selectedFrame!.id}`), // Fixed route to frames
          },
        ],
      },
    ];

    // Subscribe to frame updates
    this.subscriptions.add(
      this.framesService.updatedSource$.subscribe((frameId) => {
        this.refreshFrame(frameId, 'update');
      })
    );

    // Subscribe to frame deletions
    this.subscriptions.add(
      this.framesService.deletedSource$.subscribe((frameId) => {
        this.refreshFrame(frameId, 'delete');
      })
    );
  }

  refreshFrame(id: number, action: 'update' | 'delete'): void {
    if (id === 0) {
      // If id is 0, it indicates a full refresh is needed (e.g., after create)
      // Parent component should handle full refresh of frames
      return;
    }

    // Check if the frame exists in the current frames list
    const index = this.frames.findIndex((frame) => frame.id === id);
    if (index === -1) return; // Frame not in this list, no action needed

    if (action === 'delete') {
      // Remove the frame from the list
      this.frames = this.frames.filter((frame) => frame.id !== id);
    } else if (action === 'update') {
      // Fetch the updated frame
      this.framesService.getSingle(id).subscribe({
        next: (updatedFrame) => {
          this.frames[index] = updatedFrame;
          this.frames = [...this.frames]; // Trigger change detection
        },
        error: (error) => {
          console.error('Failed to refresh frame: ', error);
        },
      });
    }
  }

  onUpdateFrameModal(frame: Frame): void {
    this.modalService.show(UpsertFramesComponent, {
      class: 'modal-md',
      initialState: {
        id: frame.id,
      },
    });
  }

  onDeleteFrameModal(frame: Frame): void {
    this.modalService.show(DeleteFramesComponent, {
      class: 'modal-md',
      initialState: {
        id: frame.id,
        name: `${frame.program} - ${frame.intervention}`,
      },
    });
  }

  next() {
    this.first = this.first + this.rows;
  }

  prev() {
    this.first = this.first - this.rows;
  }

  reset() {
    this.first = 0;
  }

  pageChange(event: any) {
    this.first = event.first;
    this.rows = event.rows;
  }

  isLastPage(): boolean {
    return this.frames ? this.first + this.rows >= this.frames.length : true;
  }

  isFirstPage(): boolean {
    return this.frames ? this.first === 0 : true;
  }

  clearFilters() {
    this.searchValue = '';
    this.dt.reset();
  }

  downloadLogo(frame: Frame, logoType: 'program' | 'financement'): void {
    this.framesService.downloadLogo(frame.id, logoType).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${frame.program}-${logoType}-logo`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        this.sharedService.showSuccess('Logo descarregado com sucesso.');
      },
      error: (error) => {
        console.error('Failed to download logo:', error);
        this.sharedService.showError('Erro ao descarregar o logo.');
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
