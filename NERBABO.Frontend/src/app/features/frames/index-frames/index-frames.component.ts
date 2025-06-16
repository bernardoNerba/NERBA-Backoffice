import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { BsModalService } from 'ngx-bootstrap/modal';
import {
  Observable,
  combineLatest,
  debounceTime,
  distinctUntilChanged,
  map,
  startWith,
} from 'rxjs';
import { FrameService } from '../../../core/services/frame.service';
import { CommonModule } from '@angular/common';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { Frame } from '../../../core/models/frame';
import { SharedService } from '../../../core/services/shared.service';
import { CreateFramesComponent } from '../create-frames/create-frames.component';
import { UpdateFramesComponent } from '../update-frames/update-frames.component';
import { DeleteFramesComponent } from '../delete-frames/delete-frames.component';

@Component({
  selector: 'app-index-frame',
  templateUrl: './index-frames.component.html',
  styleUrl: './index-frames.component.css',
  imports: [CommonModule, ReactiveFormsModule, SpinnerComponent],
})
export class IndexFramesComponent implements OnInit {
  frames$!: Observable<Frame[]>;
  filteredFrames$!: Observable<Frame[]>;
  loading$: Observable<boolean>;
  searchControl = new FormControl('');
  columns = [
    '#',
    'Programa',
    'Intervenção',
    'Tipo Intervenção',
    'Operação',
    'Tipo Operação',
  ];

  constructor(
    private readonly frameService: FrameService,
    private readonly modalService: BsModalService,
    private readonly sharedService: SharedService
  ) {
    this.frames$ = this.frameService.frames$;
    this.loading$ = this.frameService.loading$;
  }

  ngOnInit(): void {
    this.frameService.loadFrames();

    // Set up search filtering
    this.filteredFrames$ = combineLatest([
      this.frames$,
      this.searchControl.valueChanges.pipe(
        startWith(''),
        debounceTime(300),
        distinctUntilChanged()
      ),
    ]).pipe(
      map(([frames, searchTerm]) => {
        if (!searchTerm) return frames;
        const term = searchTerm.toLowerCase();
        return frames.filter(
          (frame) =>
            frame.program.toLowerCase().includes(term) ||
            frame.intervention.toLowerCase().includes(term) ||
            frame.interventionType.toLowerCase().includes(term) ||
            frame.operation.toLowerCase().includes(term) ||
            frame.operationType.toLowerCase().includes(term)
        );
      })
    );

    this.updateBreadcumbs();
  }

  onAddFrameModal(): void {
    this.modalService.show(CreateFramesComponent, {
      class: 'modal-md',
      initialState: {},
    });
  }

  onUpdateFrameModal(frame: Frame): void {
    this.modalService.show(UpdateFramesComponent, {
      class: 'modal-md',
      initialState: {
        id: frame.id,
        frame: frame,
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

  private updateBreadcumbs(): void {
    this.sharedService.insertIntoBreadcrumb([
      {
        url: '/dashboard',
        displayName: 'Dashboard',
        className: '',
      },
      {
        url: '/frames',
        displayName: 'Enquadramentos',
        className: 'inactive',
      },
    ]);
  }
}
