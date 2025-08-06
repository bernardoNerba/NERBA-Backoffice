import { Component, Input } from '@angular/core';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-message',
  imports: [Message],
  templateUrl: './message.component.html',
  styleUrl: './message.component.css',
})
export class MessageComponent {
  @Input({ required: true }) show: boolean = false;
  @Input({ required: true }) severity: string = '';
  @Input({ required: true }) description: string = '';
  @Input() items: string[] = [];

  get severityIcon() {
    switch (this.severity) {
      case 'warn':
        return 'pi pi-exclamation-triangle';
      case 'info':
        return 'pi pi-exclamation-circle';
      case 'error':
        return 'pi pi-ban';
      default:
        return 'pi pi-info';
    }
  }

  get severityTitle() {
    switch (this.severity) {
      case 'warn':
        return 'Cuidado!';
      case 'error':
        return 'Perigo!';
      case 'info':
        return 'Saiba que:';
      default:
        return 'Informação:';
    }
  }
}
