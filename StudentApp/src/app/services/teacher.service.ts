import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Teacher {
  teacherId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  subject: string;
  password: string;
  enrollmentDate: string;
  createdDate: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class TeacherService {
  private apiUrl = '/api/teachers';

  constructor(private http: HttpClient) { }

  // Get all teachers
  getTeachers(): Observable<Teacher[]> {
    return this.http.get<Teacher[]>(this.apiUrl);
  }

  // Get single teacher by ID
  getTeacher(id: number): Observable<Teacher> {
    return this.http.get<Teacher>(`${this.apiUrl}/${id}`);
  }

  // Register new teacher
  createTeacher(teacher: Teacher): Observable<Teacher> {
    return this.http.post<Teacher>(this.apiUrl, teacher);
  }

  // Update existing teacher
  updateTeacher(id: number, teacher: Teacher): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, teacher);
  }

  // Delete teacher
  deleteTeacher(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Login teacher
  login(credentials: LoginDto): Observable<{ token: string; teacher: Teacher }> {
    return this.http.post<{ token: string; teacher: Teacher }>(`${this.apiUrl}/login`, credentials);
  }
}
