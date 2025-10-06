import { Component, Input } from '@angular/core';
import { Message } from 'primeng/message';
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';

export interface MessageAction {
  label: string;
  icon?: string;
  callback: () => void;
  severity?: 'success' | 'info' | 'warn' | 'danger' | 'help' | 'primary' | 'secondary' | 'contrast';
}

@Component({
  selector: 'app-message',
  imports: [Message, ButtonModule, CommonModule],
  templateUrl: './message.component.html',
  styleUrl: './message.component.css',
})
export class MessageComponent {
  @Input({ required: true }) show: boolean = false;
  @Input({ required: true }) severity: string = '';
  @Input({ required: true }) description: string = '';
  @Input() items: string[] = [];
  @Input() action?: MessageAction;

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

  onActionClick(): void {
    if (this.action && this.action.callback) {
      this.action.callback();
    }
  }

  getActionSeverity(): 'success' | 'info' | 'warn' | 'danger' | 'help' | 'primary' | 'secondary' | 'contrast' {
    if (this.action?.severity) {
      return this.action.severity;
    }
    // Default severity based on message severity
    switch (this.severity) {
      case 'warn':
        return 'warn';
      case 'error':
        return 'danger';
      case 'info':
        return 'info';
      default:
        return 'secondary';
    }
  }
}
