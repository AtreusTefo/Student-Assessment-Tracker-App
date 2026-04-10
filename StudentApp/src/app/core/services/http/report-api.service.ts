import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ReportApiService {
  private readonly apiUrl = '/api/reports';

  constructor(private http: HttpClient) {}

  /** Download all students as CSV (returns raw Blob). */
  exportAllStudentsCsv(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/students/csv`, { responseType: 'blob' });
  }

  /** Download one student's assessments as CSV. */
  exportStudentCsv(studentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/students/${studentId}/csv`, { responseType: 'blob' });
  }

  /** Download one student's assessment report as PDF. */
  exportStudentPdf(studentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/students/${studentId}/pdf`, { responseType: 'blob' });
  }
}
