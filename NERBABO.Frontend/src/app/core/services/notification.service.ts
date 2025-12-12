import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, interval, Observable, startWith, switchMap, tap, map } from 'rxjs';
import { Notification, NotificationCount } from '../models/notification';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { SharedService } from './shared.service';
import { OkResponse } from '../models/okResponse';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  private notificationCountSubject = new BehaviorSubject<NotificationCount>({
    totalCount: 0,
    unreadCount: 0,
  });
  private loadingSubject = new BehaviorSubject<boolean>(false);

  notifications$ = this.notificationsSubject.asObservable();
  notificationCount$ = this.notificationCountSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();

  constructor(
    private http: HttpClient,
    private sharedService: SharedService
  ) {
    // Load notification count on initialization
    this.loadNotificationCount();

    // Poll notification count every 60 seconds
    interval(60000)
      .pipe(
        startWith(0),
        switchMap(() => this.getNotificationCount())
      )
      .subscribe({
        next: (count) => {
          this.notificationCountSubject.next(count);
        },
        error: (error) => {
          console.error('Error polling notification count:', error);
        },
      });
  }

  /**
   * Gets all notifications
   */
  getAllNotifications(): Observable<Notification[]> {
    this.loadingSubject.next(true);
    return this.http.get<any>(API_ENDPOINTS.notifications).pipe(
      tap({
        next: (response: any) => {
          const notifications = response.data || [];
          this.notificationsSubject.next(notifications);
          this.loadingSubject.next(false);
        },
        error: (error) => {
          this.notificationsSubject.next([]);
          this.loadingSubject.next(false);
          this.sharedService.handleErrorResponse(error);
        },
      })
    );
  }

  /**
   * Gets only unread notifications
   */
  getUnreadNotifications(): Observable<Notification[]> {
    this.loadingSubject.next(true);
    return this.http.get<any>(API_ENDPOINTS.notification_unread).pipe(
      tap({
        next: (response: any) => {
          const notifications = response.data || [];
          this.notificationsSubject.next(notifications);
          this.loadingSubject.next(false);
        },
        error: (error) => {
          this.notificationsSubject.next([]);
          this.loadingSubject.next(false);
          // Don't show error toast if there are no unread notifications (404)
          if (error.status !== 404) {
            this.sharedService.handleErrorResponse(error);
          }
        },
      })
    );
  }

  /**
   * Gets the notification count (total and unread)
   */
  getNotificationCount(): Observable<NotificationCount> {
    return this.http.get<any>(API_ENDPOINTS.notification_count).pipe(
      map((response: any) => response.data || { totalCount: 0, unreadCount: 0 })
    );
  }

  /**
   * Loads the notification count and updates the subject
   */
  loadNotificationCount(): void {
    this.getNotificationCount().subscribe({
      next: (count) => {
        this.notificationCountSubject.next(count);
      },
      error: (error) => {
        console.error('Error loading notification count:', error);
      },
    });
  }

  /**
   * Marks a single notification as read
   */
  markAsRead(id: number): Observable<OkResponse> {
    return this.http.put<OkResponse>(API_ENDPOINTS.notification_mark_read(id), {}).pipe(
      tap({
        next: (response) => {
          this.sharedService.showSuccess(response.message);
          this.refreshNotifications();
          this.loadNotificationCount();
        },
        error: (error) => {
          this.sharedService.handleErrorResponse(error);
        },
      })
    );
  }

  /**
   * Marks all notifications as read
   */
  markAllAsRead(): Observable<OkResponse> {
    return this.http.put<OkResponse>(API_ENDPOINTS.notification_mark_all_read, {}).pipe(
      tap({
        next: (response) => {
          this.sharedService.showSuccess(response.message);
          this.refreshNotifications();
          this.loadNotificationCount();
        },
        error: (error) => {
          this.sharedService.handleErrorResponse(error);
        },
      })
    );
  }

  /**
   * Deletes a notification
   */
  deleteNotification(id: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(`${API_ENDPOINTS.notifications}delete/${id}`).pipe(
      tap({
        next: (response) => {
          this.sharedService.showSuccess(response.message);
          this.refreshNotifications();
          this.loadNotificationCount();
        },
        error: (error) => {
          this.sharedService.handleErrorResponse(error);
        },
      })
    );
  }

  /**
   * Manually triggers notification generation
   */
  generateNotifications(): Observable<OkResponse> {
    return this.http.post<OkResponse>(API_ENDPOINTS.notification_generate, {}).pipe(
      tap({
        next: (response) => {
          this.sharedService.showSuccess(response.message);
          this.refreshNotifications();
          this.loadNotificationCount();
        },
        error: (error) => {
          this.sharedService.handleErrorResponse(error);
        },
      })
    );
  }

  /**
   * Refreshes notifications (call after any modification)
   */
  refreshNotifications(): void {
    this.getAllNotifications().subscribe();
  }
}
