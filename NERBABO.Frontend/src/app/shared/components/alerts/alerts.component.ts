import { Component, OnInit } from '@angular/core';
import { Alert } from '../../../core/models/alert';
import { SharedService } from '../../../core/services/shared.service';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-alerts',
  imports: [ToastModule],
  templateUrl: './alerts.component.html',
  styleUrl: './alerts.component.css',
  providers: [MessageService],
})
export class AlertsComponent implements OnInit {
  constructor(
    private sharedService: SharedService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.sharedService.alerts$.subscribe((alert) => {
      this.addAlert(alert);
    });
  }

  addAlert(alert: Alert) {
    const severity = this.mapAlertTypeToSeverity(alert.type);

    this.messageService.add({
      severity: severity,
      summary: alert.title || this.getDefaultTitle(severity),
      detail: alert.message,
      life: alert.timeout || 1000, // Default 1 seconds if no timeout specified
      closable: alert.dismissible !== false, // Default to true if not specified
    });
  }

  private mapAlertTypeToSeverity(type: string): string {
    const typeMap: { [key: string]: string } = {
      success: 'success',
      info: 'info',
      warning: 'warn',
      danger: 'error',
      error: 'error',
      primary: 'info',
      secondary: 'info',
      light: 'info',
      dark: 'info',
    };

    return typeMap[type] || 'info';
  }

  private getDefaultTitle(severity: string): string {
    const titleMap: { [key: string]: string } = {
      success: 'Success',
      info: 'Info',
      warn: 'Warning',
      error: 'Error',
    };

    return titleMap[severity] || 'Notification';
  }

  // Optional: Method to manually clear all toasts
  clearAllToasts() {
    this.messageService.clear();
  }
}
