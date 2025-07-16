import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Observable } from 'rxjs';
import { FrameService } from '../../../core/services/frame.service';
import { CommonModule } from '@angular/common';
import { Frame } from '../../../core/models/frame';
import { SharedService } from '../../../core/services/shared.service';
import { CreateFramesComponent } from '../create-frames/create-frames.component';
import { UpdateFramesComponent } from '../update-frames/update-frames.component';
import { DeleteFramesComponent } from '../delete-frames/delete-frames.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';
import { ICONS } from '../../../core/objects/icons';
import { FramesTableComponent } from '../../../shared/components/tables/frames-table/frames-table.component';

@Component({
  selector: 'app-index-frame',
  templateUrl: './index-frames.component.html',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    IconComponent,
    FramesTableComponent,
  ],
})
export class IndexFramesComponent implements OnInit {
  frames$!: Observable<Frame[]>;
  loading$: Observable<boolean>;
  ICONS = ICONS;

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
