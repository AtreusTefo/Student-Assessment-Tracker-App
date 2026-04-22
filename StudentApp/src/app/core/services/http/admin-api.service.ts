import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AdminDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  createdAt: string;
}

export interface AdminLoginDto {
  email: string;
  password: string;
}

export interface AdminLoginResponse {
  token: string;
  admin: AdminDto;
}

export interface AuditLogDto {
  id: number;
  entityName: string;
  entityId: number;
  action: string;
  oldValues: string | null;
  newValues: string | null;
  changedBy: string | null;
  changedByRole: string | null;
  changedAt: string;
}

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly apiUrl = '/api/admins';

  constructor(private http: HttpClient) {}

  login(dto: AdminLoginDto): Observable<AdminLoginResponse> {
    return this.http.post<AdminLoginResponse>(`${this.apiUrl}/login`, dto);
  }

  // ─── Teacher management ───────────────────────────────────────────────────

  getAllTeachers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/teachers`);
  }

  /**
   * Create a teacher account (passwordless — teacher activates via /api/teachers/activate)
   * POST /api/teachers
   */
  createTeacher(dto: any): Observable<any> {
    return this.http.post<any>('/api/teachers', dto);
  }

  deleteTeacher(teacherId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/teachers/${teacherId}`);
  }

  resetTeacherPassword(teacherId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/teachers/${teacherId}/reset-password`, {});
  }

  updateTeacher(teacherId: number, dto: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/teachers/${teacherId}`, dto);
  }

  // ─── Student management ───────────────────────────────────────────────────

  getAllStudents(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/students`);
  }

  /**
   * Create a student account (passwordless — student activates via /api/students/activate)
   * POST /api/students
   */
  createStudent(dto: any): Observable<any> {
    return this.http.post<any>('/api/students', dto);
  }

  deleteStudent(studentId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/students/${studentId}`);
  }

  resetStudentPassword(studentId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/students/${studentId}/reset-password`, {});
  }

  updateStudent(studentId: number, dto: any): Observable<any> {
    return this.http.put<any>(`/api/students/${studentId}`, dto);
  }

  // ─── Bulk import ──────────────────────────────────────────────────────────

  /**
   * Bulk-import students from a parsed JSON row array.
   * POST /api/admins/students/bulk
   */
  bulkImportStudents(rows: any[]): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/students/bulk`, rows);
  }

  /**
   * Bulk-import teachers from a parsed JSON row array.
   * POST /api/admins/teachers/bulk
   */
  bulkImportTeachers(rows: any[]): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/teachers/bulk`, rows);
  }

  /**
   * Bulk-import students from a CSV file upload.
   * POST /api/admins/students/bulk-csv  (multipart/form-data)
   */
  bulkImportStudentsCsv(file: File): Observable<any> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<any>(`${this.apiUrl}/students/bulk-csv`, form);
  }

  /**
   * Bulk-import teachers from a CSV file upload.
   * POST /api/admins/teachers/bulk-csv  (multipart/form-data)
   */
  bulkImportTeachersCsv(file: File): Observable<any> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<any>(`${this.apiUrl}/teachers/bulk-csv`, form);
  }

  // ─── Teacher–Student assignment ───────────────────────────────────────────

  /**
   * Assign a teacher to a student
   * POST /api/students/{studentId}/teachers/{teacherId}
   */
  assignStudentToTeacher(studentId: number, teacherId: number): Observable<any> {
    return this.http.post<any>(`/api/students/${studentId}/teachers/${teacherId}`, {});
  }

  /**
   * Remove a teacher from a student
   * DELETE /api/students/{studentId}/teachers/{teacherId}
   */
  unassignStudentFromTeacher(studentId: number, teacherId: number): Observable<any> {
    return this.http.delete<any>(`/api/students/${studentId}/teachers/${teacherId}`);
  }

  // ─── Reference data ───────────────────────────────────────────────────────

  getSubjects(): Observable<any[]> {
    return this.http.get<any[]>('/api/subjects');
  }

  getGrades(): Observable<any[]> {
    return this.http.get<any[]>('/api/grades');
  }

  // ─── Audit logs ──────────────────────────────────────────────────────────

  getAuditLogs(page = 1, pageSize = 50): Observable<AuditLogDto[]> {
    return this.http.get<AuditLogDto[]>(
      `${this.apiUrl}/audit-logs?page=${page}&pageSize=${pageSize}`
    );
  }

  getAuditLogsByEntity(entityName: string, entityId: number): Observable<AuditLogDto[]> {
    return this.http.get<AuditLogDto[]>(
      `${this.apiUrl}/audit-logs/${entityName}/${entityId}`
    );
  }
}
