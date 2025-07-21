import { Component, Input, OnInit } from '@angular/core';
import { IView } from '../../../core/interfaces/IView';
import { PeopleService } from '../../../core/services/people.service';

@Component({
  selector: 'app-view-teacher',
  imports: [],
  templateUrl: './view-teacher.component.html',
})
export class ViewTeacherComponent implements OnInit {
  @Input({ required: true }) personId!: number;

  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }
}
