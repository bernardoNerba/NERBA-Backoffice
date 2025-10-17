import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { BsModalService } from 'ngx-bootstrap/modal';
import { Observable } from 'rxjs';
import { FrameService } from '../../../core/services/frame.service';
import { CommonModule } from '@angular/common';
import { Frame } from '../../../core/models/frame';
import { SharedService } from '../../../core/services/shared.service';

import { ICONS } from '../../../core/objects/icons';
import { FramesTableComponent } from '../../../shared/components/tables/frames-table/frames-table.component';
import { IIndex } from '../../../core/interfaces/IIndex';
import { UpsertFramesComponent } from '../upsert-frames/upsert-frames.component';
import { TitleComponent } from '../../../shared/components/title/title.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-index-frame',
  templateUrl: './index-frames.component.html',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FramesTableComponent,
    TitleComponent,
  ],
})
export class IndexFramesComponent implements OnInit, IIndex {
  frames$!: Observable<Frame[]>;
  loading$: Observable<boolean>;
  ICONS = ICONS;

  constructor(
    private readonly frameService: FrameService,
    private readonly modalService: BsModalService,
    private readonly sharedService: SharedService,
    public readonly authService: AuthService
  ) {
    this.frames$ = this.frameService.frames$;
    this.loading$ = this.frameService.loading$;
  }

  ngOnInit(): void {
    this.frameService.loadFrames();
    this.updateBreadcrumbs();
  }

  onCreateModal(): void {
    this.modalService.show(UpsertFramesComponent, {
      class: 'modal-md',
      initialState: {
        id: 0,
      },
    });
  }

  updateBreadcrumbs(): void {
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
