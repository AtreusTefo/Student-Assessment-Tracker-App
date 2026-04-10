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

  getAllTeachers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/teachers`);
  }

  deleteTeacher(teacherId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/teachers/${teacherId}`);
  }

  getAllStudents(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/students`);
  }

  deleteStudent(studentId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/students/${studentId}`);
  }

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
