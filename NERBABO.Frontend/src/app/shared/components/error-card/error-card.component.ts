import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-error-card',
  imports: [],
  templateUrl: './error-card.component.html',
  styleUrl: './error-card.component.css',
})
export class ErrorCardComponent {
  @Input({ required: true }) errors!: Array<string>;
  @Input({ required: true }) submitted: boolean = false;
}
