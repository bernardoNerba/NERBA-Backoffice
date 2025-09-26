import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, tap } from 'rxjs';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';

export interface TeacherPayment {
  moduleTeachingId: number;
  moduleId: number;
  moduleName: string;
  teacherPersonId: number;
  teacherName: string;
  paymentTotal: number;
  calculatedTotal: number;
  paymentDate: string;
  paymentProcessed: boolean;
}

export interface UpdateTeacherPayment {
  moduleTeachingId: number;
  paymentTotal: number;
  paymentDate: string;
}

export interface StudentPayment {
  actionEnrollmentId: number;
  actionId: number;
  actionTitle: string;
  studentPersonId: number;
  studentName: string;
  paymentTotal: number;
  calculatedTotal: number;
  paymentDate: string;
  paymentProcessed: boolean;
}

export interface UpdateStudentPayment {
  actionEnrollmentId: number;
  paymentTotal: number;
  paymentDate: string;
}

@Injectable({
  providedIn: 'root',
})
export class PaymentsService {
  private updatedSource = new Subject<void>();
  updated$ = this.updatedSource.asObservable();

  constructor(private http: HttpClient) {}

  /**
   * Gets all teacher payments for a specific action
   */
  getTeacherPaymentsByActionId(actionId: number): Observable<TeacherPayment[]> {
    return this.http.get<TeacherPayment[]>(
      `${API_ENDPOINTS.teacherPayments}${actionId}`
    );
  }

  /**
   * Updates teacher payment information
   */
  updateTeacherPayments(payment: UpdateTeacherPayment): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(
        API_ENDPOINTS.teacherPayments,
        payment
      )
      .pipe(
        tap(() => {
          this.notifyPaymentUpdated();
        })
      );
  }

  /**
   * Gets all student payments for a specific action
   */
  getStudentPaymentsByActionId(actionId: number): Observable<StudentPayment[]> {
    return this.http.get<StudentPayment[]>(
      `${API_ENDPOINTS.payments}students/${actionId}`
    );
  }

  /**
   * Updates student payment information
   */
  updateStudentPayments(payment: UpdateStudentPayment): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(
        `${API_ENDPOINTS.payments}students/`,
        payment
      )
      .pipe(
        tap(() => {
          this.notifyPaymentUpdated();
        })
      );
  }

  private notifyPaymentUpdated(): void {
    this.updatedSource.next();
  }
}