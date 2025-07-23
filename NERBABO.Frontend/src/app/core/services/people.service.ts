import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { Person } from '../models/person';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { OkResponse } from '../models/okResponse';
import { PersonRelationship } from '../models/personRelationships';
import { finalize, tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class PeopleService {
  private peopleSubject = new BehaviorSubject<Person[]>([]);
  private peopleWithoutUserSubject = new BehaviorSubject<Person[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();

  people$ = this.peopleSubject.asObservable();
  peopleWithoutUser$ = this.peopleWithoutUserSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();

  constructor(private http: HttpClient, private sharedService: SharedService) {
    this.fetchPeople();
    this.fetchPeopleWithoutUser();
  }

  upsertPerson(model: Person, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) {
      return this.updatePerson(model.id, model);
    }
    return this.createPerson(model);
  }

  private createPerson(
    model: Omit<Person, 'id' | 'fullName' | 'age'>
  ): Observable<OkResponse> {
    return this.http
      .post<OkResponse>(`${API_ENDPOINTS.all_people}create`, model)
      .pipe(tap(() => this.notifyPersonUpdate(0))); // Notify full refresh after create
  }

  private updatePerson(
    id: number,
    model: Omit<Person, 'fullName' | 'age'>
  ): Observable<OkResponse> {
    return this.http
      .put<OkResponse>(`${API_ENDPOINTS.all_people}update/${id}`, model)
      .pipe(tap(() => this.notifyPersonUpdate(id))); // Notify update after success
  }

  deletePerson(id: number): Observable<OkResponse> {
    return this.http
      .delete<OkResponse>(`${API_ENDPOINTS.all_people}delete/${id}`)
      .pipe(tap(() => this.notifyPersonDelete(id))); // Notify delete after success
  }

  getSinglePerson(id: number): Observable<Person> {
    return this.http.get<Person>(`${API_ENDPOINTS.all_people}${id}`);
  }

  getPersonRelationships(id: number): Observable<PersonRelationship> {
    return this.http.get<PersonRelationship>(
      `${API_ENDPOINTS.all_people}${id}/relationships`
    );
  }

  notifyPersonUpdate(personId: number) {
    this.updatedSource.next(personId);
  }

  notifyPersonDelete(personId: number) {
    this.deletedSource.next(personId);
  }

  get hasPeopleData(): boolean {
    return this.peopleSubject.getValue().length > 0;
  }

  triggerFetchPeople() {
    this.fetchPeople();
  }

  personById(id: number): Person | undefined {
    return this.peopleSubject.getValue()?.find((value) => value.id === id);
  }

  private fetchPeople(): void {
    this.loadingSubject.next(true);
    this.http
      .get<Person[]>(API_ENDPOINTS.all_people)
      .pipe(finalize(() => this.loadingSubject.next(false)))
      .subscribe({
        next: (data: Person[]) => {
          this.peopleSubject.next(data);
        },
        error: (err) => {
          console.error('Failed to fetch people:', err);
          this.peopleSubject.next([]);
          if (err.status === 403 || err.status === 401) {
            this.sharedService.redirectUser();
          }
        },
      });
  }

  private fetchPeopleWithoutUser(): void {
    this.http
      .get<Person[]>(API_ENDPOINTS.people_not_user + 'colaborator/')
      .pipe(finalize(() => this.loadingSubject.next(false)))
      .subscribe({
        next: (data: Person[]) => {
          this.peopleWithoutUserSubject.next(data);
        },
        error: (err) => {
          console.error('Failed to fetch people without user:', err);
          this.peopleWithoutUserSubject.next([]);
          if (err.status === 403 || err.status === 401) {
            this.sharedService.redirectUser();
          }
        },
      });
  }
}
