import { Component, NgModule, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  NgModel,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { CoursesService } from '../../../core/services/courses.service';
import { SharedService } from '../../../core/services/shared.service';
import { ErrorCardComponent } from '../../../shared/components/error-card/error-card.component';
import { CommonModule } from '@angular/common';
import { STATUS, StatusEnum } from '../../../core/objects/status';
import {
  HabilitationEnum,
  HABILITATIONS,
} from '../../../core/objects/habilitations';
import { DESTINATORS } from '../../../core/objects/destinators';
import { FrameService } from '../../../core/services/frame.service';
import { Observable } from 'rxjs';
import { Frame } from '../../../core/models/frame';
import { MultiSelectModule } from 'primeng/multiselect';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { SelectModule } from 'primeng/select';

@Component({
  selector: 'app-create-courses',
  imports: [
    ErrorCardComponent,
    CommonModule,
    ReactiveFormsModule,
    MultiSelectModule,
    AutoCompleteModule, // Added AutoCompleteModule to imports
    SelectModule,
  ],
  templateUrl: './create-courses.component.html',
  styleUrl: './create-courses.component.css',
})
export class CreateCoursesComponent implements OnInit {
  errorMessages: string[] = [];
  form: FormGroup = new FormGroup({});
  submitted: boolean = false;
  loading: boolean = false;
  frames$!: Observable<Frame[]>;
  frames: Frame[] = []; // Array to hold all frames
  filteredFrames: Frame[] = []; // Array for filtered autocomplete results

  STATUS = STATUS;
  HABILITATIONS = HABILITATIONS;
  DESTINATORS = DESTINATORS;

  constructor(
    public bsModalRef: BsModalRef,
    private formBuilder: FormBuilder,
    private coursesService: CoursesService,
    private sharedService: SharedService,
    private frameService: FrameService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.frames$ = this.frameService.frames$;
    this.loadFrames();
  }

  private initializeForm() {
    this.form = this.formBuilder.group({
      frameId: ['', [Validators.required]],
      title: [
        '',
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(255),
        ],
      ],
      objectives: ['', [Validators.minLength(3), Validators.maxLength(510)]],
      destinators: [[]],
      area: ['', [Validators.minLength(3), Validators.maxLength(55)]],
      totalDuration: [
        '',
        [Validators.required, Validators.min(0), Validators.max(1000)],
      ],
      status: ['Não Iniciado'],
      minHabilitationLevel: ['Sem Comprovativo'],
    });
  }

  private loadFrames() {
    // Subscribe to frames$ to populate the local arrays
    this.frames$.subscribe((frames) => {
      this.frames = frames;
      this.filteredFrames = [...frames]; // Initialize filtered frames
    });
  }

  filterFrames(event: any) {
    const query = event.query.toLowerCase();
    this.filteredFrames = this.frames.filter(
      (frame) =>
        frame.program.toLowerCase().includes(query) ||
        (frame.program && frame.program.toLowerCase().includes(query))
    );
  }

  onSubmit() {
    this.submitted = true;
    if (this.form.valid) {
      const selectedFrame = this.form.get('frameId')?.value;
      const selectedFrameId = selectedFrame?.id || selectedFrame; // Handle both object and ID cases
      console.log('Selected frame ID:', selectedFrameId);
      console.log('Selected frame object:', selectedFrame);

      // When submitting to your API, use selectedFrameId
      const formData = {
        ...this.form.value,
        frameId: selectedFrameId, // Send only the ID
      };

      // Handle form submission with formData
    }
  }
}
