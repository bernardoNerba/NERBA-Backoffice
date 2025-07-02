import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { Person } from '../models/person';
import { HttpClient } from '@angular/common/http';
import { SharedService } from './shared.service';
import { environment } from '../../../environments/environment.development';
import { OkResponse } from '../models/okResponse';
import { API_ENDPOINTS } from '../objects/apiEndpoints';
import { PersonRelationship } from '../models/personRelationships';

@Injectable({
  providedIn: 'root',
})
export class PeopleService {
  private peopleSubject = new BehaviorSubject<Array<Person>>([]);
  private peopleWithoutUserSubject = new BehaviorSubject<Array<Person>>([]);
  private loadingSubject = new BehaviorSubject<boolean>(true);
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

  createPerson(model: Omit<Person, 'id' | 'fullName'>): Observable<OkResponse> {
    return this.http.post<OkResponse>(
      `${environment.appUrl}/api/people/create`,
      model
    );
  }

  updatePerson(
    id: number,
    model: Omit<Person, 'fullName' | 'age'>
  ): Observable<OkResponse> {
    return this.http.put<OkResponse>(
      `${environment.appUrl}/api/people/update/${id}`,
      model
    );
  }

  deletePerson(id: number) {
    return this.http.delete<OkResponse>(
      `${environment.appUrl}/api/people/delete/${id}`
    );
  }

  getSinglePerson(id: number) {
    return this.http.get<Person>(`${environment.appUrl}/api/people/${id}`);
  }

  getPersonRelationships(id: number) {
    return this.http.get<PersonRelationship>(`${environment.appUrl}/api/people/${id}/relationships`);
  }

  notifyPersonUpdate(personId: number) {
    this.updatedSource.next(personId);
  }

  notifyPersonDelete(personId: number) {
    this.deletedSource.next(personId);
  }

  get hasPeopleData() {
    return this.peopleSubject.getValue().length > 0;
  }

  triggerFetchPeople() {
    this.fetchPeople();
  }

  personById(id: number) {
    return this.peopleSubject.getValue()?.find((value) => value.id === id);
  }

  private fetchPeople(): void {
    this.loadingSubject.next(true);

    this.http.get<Array<Person>>(API_ENDPOINTS.all_people).subscribe({
      next: (data: Person[]) => {
        // store data on the observable
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
    this.loadingSubject.next(false);
  }

  private fetchPeopleWithoutUser(): void {
    this.http.get<Array<Person>>(API_ENDPOINTS.people_not_user).subscribe({
      next: (data: Person[]) => {
        // store data on the observable
        this.peopleWithoutUserSubject.next(data);
      },
      error: (err) => {
        console.error('Failed to fetch people:', err);
        this.peopleWithoutUserSubject.next([]);
        if (err.status === 403 || err.status === 401) {
          this.sharedService.redirectUser();
        }
      },
    });
  }
}
