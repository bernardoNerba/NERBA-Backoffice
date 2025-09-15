import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-process-mt-payments',
  imports: [],
  templateUrl: './process-mt-payments.component.html',
  styleUrl: './process-mt-payments.component.css',
})
export class ProcessMtPaymentsComponent {
  @Input({ required: true }) actionId!: number;
}
