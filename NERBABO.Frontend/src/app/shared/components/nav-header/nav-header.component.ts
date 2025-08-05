import { Component, Input } from '@angular/core';
import { Person } from '../../../core/models/person';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-nav-header',
  imports: [RouterLink],
  template: `<ul class="nav nav-tabs card-header-tabs">
    <li class="nav-item">
      <a
        class="nav-link {{ activePage === 'person' ? 'active' : '' }}"
        aria-current="true"
        [routerLink]="['/people', person.id]"
        >Pessoa</a
      >
    </li>
    @if(person.isStudent){
    <li class="nav-item">
      <a
        class="nav-link {{ activePage === 'student' ? 'active' : '' }}"
        [routerLink]="['/people', person.id, 'student']"
        >Formando</a
      >
    </li>
    } @if(person.isTeacher){
    <li class="nav-item">
      <a
        class="nav-link {{ activePage === 'teacher' ? 'active' : '' }}"
        [routerLink]="['/people', person.id, 'teacher']"
        >Formador</a
      >
    </li>
    } @if(person.isColaborator){
    <li class="nav-item">
      <a
        class="nav-link {{ activePage === 'collaborator' ? 'active' : '' }}"
        [routerLink]="['/people', person.id, 'acc']"
        >Colaborador</a
      >
    </li>
    }
  </ul>`,
})
export class NavHeaderComponent {
  @Input({ required: true }) person!: Person;
  @Input({ required: true }) activePage!: string;
}
