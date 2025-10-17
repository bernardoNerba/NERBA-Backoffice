import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Alert } from '../models/alert';
import { Subject } from 'rxjs';
import { NavigationLink } from '../models/navigationLink';

@Injectable({
  providedIn: 'root',
})
export class SharedService {
  private alertSubject = new Subject<Alert>();
  private breadcrumbList = new Subject<Array<NavigationLink>>();

  alerts$ = this.alertSubject.asObservable();
  breadcrumbList$ = this.breadcrumbList.asObservable();

  isCollapsed: boolean = false;

  constructor(private router: Router) {}

  insertIntoBreadcrumb(list: Array<NavigationLink>) {
    this.breadcrumbList.next(list);
  }

  show(alert: Alert) {
    this.alertSubject.next(alert);
  }

  showSuccess(message: string, dismissible = true, timeout = 5000) {
    this.show({ type: 'success', message, dismissible, timeout });
  }

  showError(message: string, dismissible = true, timeout = 5000) {
    this.show({ type: 'danger', message, dismissible, timeout });
  }

  showWarning(message: string, dismissible = true, timeout = 5000) {
    this.show({ type: 'warning', message, dismissible, timeout });
  }

  showInfo(message: string, dismissible = true, timeout = 5000) {
    this.show({ type: 'info', message, dismissible, timeout });
  }

  redirectUser(url: string = '/') {
    this.router.navigateByUrl(url);
    this.showError('Não tem acesso ou ocorreu um erro ao retornar os dados.');
  }

  handleErrorResponse(error: any): Array<string> {
    let errorMessages = [];

    if (error.status === 403 || error.status === 401) {
      // no error but status code 40...
      this.showError('Não podes realizar esta ação.');
    }

    if (Array.isArray(error.error.errors)) {
      // many errors
      errorMessages = error.error.errors;
    } else if (error.error !== null) {
      // only one error
      errorMessages.push(error.error.detail);
    } else {
      this.showError(
        // unexpected error
        'Algo de errado ocorreu volte a tentar mais tarde.'
      );
    }

    if (error.error.detail) {
      this.showError(error.error.detail);
    } else {
      this.showError(error.error.title);
    }

    return errorMessages;
  }
}
