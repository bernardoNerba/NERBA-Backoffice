import { Component, OnInit } from '@angular/core';
import { Alert } from '../../../core/models/alert';
import { SharedService } from '../../../core/services/shared.service';
import { AlertModule } from 'ngx-bootstrap/alert';

@Component({
  selector: 'app-alerts',
  imports: [AlertModule],
  templateUrl: './alerts.component.html',
  styleUrl: './alerts.component.css',
})
export class AlertsComponent implements OnInit {
  alerts: Alert[] = [];

  constructor(private sharedService: SharedService) {}

  ngOnInit(): void {
    this.sharedService.alerts$.subscribe((alert) => {
      this.addAlert(alert);
    });
  }

  addAlert(alert: Alert) {
    this.alerts.unshift(alert);

    if (alert.timeout) {
      setTimeout(() => {
        this.removeAlert(this.alerts.indexOf(alert));
      }, alert.timeout);
    }
  }

  removeAlert(index: number) {
    this.alerts.splice(index, 1);
  }
}
