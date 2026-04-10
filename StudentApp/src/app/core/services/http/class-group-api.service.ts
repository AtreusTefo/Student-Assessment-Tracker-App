import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ClassGroupDto {
  id: number;
  name: string;
  subjectId: number;
  subjectName: string;
  gradeId: number;
  gradeName: string;
  teacherId: number;
  createdAt: string;
  studentCount: number;
  students: ClassGroupMemberDto[];
}

export interface ClassGroupMemberDto {
  studentId: number;
  studentUniqueId: string;
  fullName: string;
  enrolledAt: string;
}

export interface CreateClassGroupDto {
  name: string;
  subjectId: number;
  gradeId: number;
}

@Injectable({ providedIn: 'root' })
export class ClassGroupApiService {
  private readonly apiUrl = '/api/class-groups';

  constructor(private http: HttpClient) {}

  getAll(): Observable<ClassGroupDto[]> {
    return this.http.get<ClassGroupDto[]>(this.apiUrl);
  }

  getById(id: number): Observable<ClassGroupDto> {
    return this.http.get<ClassGroupDto>(`${this.apiUrl}/${id}`);
  }

  create(dto: CreateClassGroupDto): Observable<ClassGroupDto> {
    return this.http.post<ClassGroupDto>(this.apiUrl, dto);
  }

  update(id: number, name: string): Observable<ClassGroupDto> {
    return this.http.put<ClassGroupDto>(`${this.apiUrl}/${id}`, { name });
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  enrollStudent(classGroupId: number, studentId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${classGroupId}/students`, { studentId });
  }

  unenrollStudent(classGroupId: number, studentId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${classGroupId}/students/${studentId}`);
  }
}
