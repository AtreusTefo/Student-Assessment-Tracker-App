import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AssessmentSubmissionDto } from '../../models';

/**
 * HTTP LAYER - Assessment Submission API Service
 * Handles file upload and retrieval for student assessment submissions.
 * Route: /api/students/{studentId}/assessments/{assessmentId}/submissions
 */
@Injectable({
  providedIn: 'root'
})
export class AssessmentSubmissionApiService {
  private http = inject(HttpClient);
  private readonly base = '/api/students';

  private url(studentId: number, assessmentId: number): string {
    return `${this.base}/${studentId}/assessments/${assessmentId}/submissions`;
  }

  /** Upload a file for an assessment (Student role required) */
  upload(studentId: number, assessmentId: number, file: File): Observable<AssessmentSubmissionDto> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<AssessmentSubmissionDto>(this.url(studentId, assessmentId), formData);
  }

  /** Retrieve all submissions for an assessment (Teacher role required) */
  getAll(studentId: number, assessmentId: number): Observable<AssessmentSubmissionDto[]> {
    return this.http.get<AssessmentSubmissionDto[]>(this.url(studentId, assessmentId));
  }

  /** Download a submission file as a Blob (Teacher or Student) */
  download(studentId: number, assessmentId: number, submissionId: number): Observable<Blob> {
    return this.http.get(
      `${this.url(studentId, assessmentId)}/${submissionId}/download`,
      { responseType: 'blob' }
    );
  }

  /** Delete a submission (Teacher or Student) */
  delete(studentId: number, assessmentId: number, submissionId: number): Observable<void> {
    return this.http.delete<void>(`${this.url(studentId, assessmentId)}/${submissionId}`);
  }
}
