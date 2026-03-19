import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  StudentAssessmentDto,
  CreateStudentAssessmentDto,
  UpdateStudentAssessmentDto
} from '../../models';

/**
 * DATA ACCESS LAYER - Student Assessment HTTP API Service
 * Manages the /api/students/{studentId}/assessments nested resource.
 * Allows updating a single assessment score without touching the student row.
 */
@Injectable({
  providedIn: 'root'
})
export class StudentAssessmentApiService {
  private baseUrl(studentId: number): string {
    return `/api/students/${studentId}/assessments`;
  }

  constructor(private http: HttpClient) { }

  getAll(studentId: number): Observable<StudentAssessmentDto[]> {
    return this.http.get<StudentAssessmentDto[]>(this.baseUrl(studentId));
  }

  getById(studentId: number, assessmentId: number): Observable<StudentAssessmentDto> {
    return this.http.get<StudentAssessmentDto>(`${this.baseUrl(studentId)}/${assessmentId}`);
  }

  create(studentId: number, dto: CreateStudentAssessmentDto): Observable<StudentAssessmentDto> {
    return this.http.post<StudentAssessmentDto>(this.baseUrl(studentId), dto);
  }

  update(studentId: number, assessmentId: number, dto: UpdateStudentAssessmentDto): Observable<StudentAssessmentDto> {
    return this.http.put<StudentAssessmentDto>(`${this.baseUrl(studentId)}/${assessmentId}`, dto);
  }

  delete(studentId: number, assessmentId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl(studentId)}/${assessmentId}`);
  }
}
