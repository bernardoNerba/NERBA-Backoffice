import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable, Subject } from "rxjs";
import { Person } from "../models/person";
import { HttpClient } from "@angular/common/http";
import { SharedService } from "./shared.service";
import { API_ENDPOINTS } from "../objects/apiEndpoints";
import { OkResponse } from "../models/okResponse";
import { PersonRelationship } from "../models/personRelationships";
import { finalize, tap } from "rxjs/operators";

@Injectable({
  providedIn: "root",
})
export class PeopleService {
  private peopleSubject = new BehaviorSubject<Person[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private updatedSource = new Subject<number>();
  private deletedSource = new Subject<number>();

  people$ = this.peopleSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  updatedSource$ = this.updatedSource.asObservable();
  deletedSource$ = this.deletedSource.asObservable();

  constructor(
    private http: HttpClient,
    private sharedService: SharedService,
  ) {
    this.fetchPeople();
  }

  upsertPerson(model: Person, isUpdate: boolean): Observable<OkResponse> {
    if (isUpdate) {
      return this.updatePerson(model.id, model);
    }
    return this.createPerson(model);
  }

  createPersonWithFiles(
    model: Omit<Person, "id" | "fullName" | "age">,
    habilitationPdf: File | null,
    ibanPdf: File | null,
    identificationDocumentPdf: File | null
  ): Observable<OkResponse> {
    const formData = new FormData();

    // Append all person fields
    Object.keys(model).forEach((key) => {
      const value = (model as any)[key];
      if (value !== null && value !== undefined && value !== '') {
        formData.append(key, value);
      }
    });

    // Append files if provided
    if (habilitationPdf) {
      formData.append('habilitationPdf', habilitationPdf);
    }
    if (ibanPdf) {
      formData.append('ibanPdf', ibanPdf);
    }
    if (identificationDocumentPdf) {
      formData.append('identificationDocumentPdf', identificationDocumentPdf);
    }

    return this.http
      .post<OkResponse>(`${API_ENDPOINTS.all_people}create-with-files`, formData)
      .pipe(tap(() => this.notifyPersonUpdate(0))); // Notify full refresh after create
  }

  private createPerson(
    model: Omit<Person, "id" | "fullName" | "age">,
  ): Observable<OkResponse> {
    return this.http
      .post<OkResponse>(`${API_ENDPOINTS.all_people}create`, model)
      .pipe(tap(() => this.notifyPersonUpdate(0))); // Notify full refresh after create
  }

  private updatePerson(
    id: number,
    model: Omit<Person, "fullName" | "age">,
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
      `${API_ENDPOINTS.all_people}${id}/relationships`,
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
          console.error("Failed to fetch people:", err);
          this.peopleSubject.next([]);
          if (err.status === 403 || err.status === 401) {
            this.sharedService.redirectUser();
          }
        },
      });
  }

  fetchPeopleWithoutUser(): Observable<Person[]> {
    return this.http.get<Person[]>(
      API_ENDPOINTS.people_not_user + "colaborator/",
    );
  }

  // PDF-related methods
  uploadHabilitationPdf(personId: number, file: File): Observable<OkResponse> {
    const formData = new FormData();
    formData.append('file', file);
    
    return this.http.post<OkResponse>(
      `${API_ENDPOINTS.all_people}${personId}/habilitation-pdf`, 
      formData
    ).pipe(
      tap(() => this.notifyPersonUpdate(personId))
    );
  }

  // IBAN PDF operations
  uploadIbanPdf(personId: number, file: File): Observable<Person> {
    const formData = new FormData();
    formData.append('file', file);
    
    return this.http.post<Person>(
      `${API_ENDPOINTS.all_people}${personId}/iban-pdf`, 
      formData
    ).pipe(
      tap(() => this.notifyPersonUpdate(personId))
    );
  }

  downloadIbanPdf(personId: number): Observable<Blob> {
    return this.http.get(
      `${API_ENDPOINTS.all_people}${personId}/iban-pdf`, 
      { responseType: 'blob' }
    );
  }

  deleteIbanPdf(personId: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(
      `${API_ENDPOINTS.all_people}${personId}/iban-pdf`
    ).pipe(
      tap(() => this.notifyPersonUpdate(personId))
    );
  }

  downloadHabilitationPdf(personId: number): Observable<Blob> {
    return this.http.get(
      `${API_ENDPOINTS.all_people}${personId}/habilitation-pdf`, 
      { responseType: 'blob' }
    );
  }

  deleteHabilitationPdf(personId: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(
      `${API_ENDPOINTS.all_people}${personId}/habilitation-pdf`
    ).pipe(
      tap(() => this.notifyPersonUpdate(personId))
    );
  }


  // Identification Document PDF operations
  uploadIdentificationDocumentPdf(personId: number, file: File): Observable<Person> {
    const formData = new FormData();
    formData.append('file', file);
    
    return this.http.post<Person>(
      `${API_ENDPOINTS.all_people}${personId}/identification-document-pdf`, 
      formData
    ).pipe(
      tap(() => this.notifyPersonUpdate(personId))
    );
  }

  downloadIdentificationDocumentPdf(personId: number): Observable<Blob> {
    return this.http.get(
      `${API_ENDPOINTS.all_people}${personId}/identification-document-pdf`, 
      { responseType: 'blob' }
    );
  }

  deleteIdentificationDocumentPdf(personId: number): Observable<OkResponse> {
    return this.http.delete<OkResponse>(
      `${API_ENDPOINTS.all_people}${personId}/identification-document-pdf`
    ).pipe(
      tap(() => this.notifyPersonUpdate(personId))
    );
  }

  // Bulk Import operations
  importPeopleFromCsv(file: File, stopOnFirstError: boolean = false, batchSize: number = 100): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<any>(
      `${API_ENDPOINTS.all_people}import/csv?stopOnFirstError=${stopOnFirstError}&batchSize=${batchSize}`,
      formData
    ).pipe(
      tap(() => this.triggerFetchPeople())
    );
  }

  importPeopleFromExcel(file: File, stopOnFirstError: boolean = false, batchSize: number = 100): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<any>(
      `${API_ENDPOINTS.all_people}import/excel?stopOnFirstError=${stopOnFirstError}&batchSize=${batchSize}`,
      formData
    ).pipe(
      tap(() => this.triggerFetchPeople())
    );
  }

  downloadCsvTemplate(): Observable<Blob> {
    return this.http.get(
      `${API_ENDPOINTS.all_people}import/template/csv`,
      { responseType: 'blob' }
    );
  }

  downloadExcelTemplate(): Observable<Blob> {
    return this.http.get(
      `${API_ENDPOINTS.all_people}import/template/excel`,
      { responseType: 'blob' }
    );
  }
}
