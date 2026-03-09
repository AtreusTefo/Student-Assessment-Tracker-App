import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface Student {
  studentId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  grade: string;
  enrollmentDate: string;
  assessment1: number;
  assessment2: number;
  assessment3: number;
  createdDate: string;
}

export interface StudentListDto {
  id: number;
  firstName: string;
  lastName: string;
}

export interface StudentDetailDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  grade: string;
  createdAt: string;
  assessment1: number;
  assessment2: number;
  assessment3: number;
  totalScore: number;
  averageScore: number;
  percentage: number;
  performanceLevel: string;
}

@Injectable({
  providedIn: 'root'
})
export class StudentService {
  private apiUrl = '/api/students';

  constructor(private http: HttpClient) { }

  // Get all students (returns StudentListDto with minimal fields)
  getStudents(): Observable<StudentListDto[]> {
    return this.http.get<StudentListDto[]>(this.apiUrl).pipe(
      // Ensure id property is properly mapped if needed
      map(students => students.map(s => ({
        ...s,
        id: s.id || (s as any).studentId // Fallback in case of format mismatch
      })))
    );
  }

  // Get single student by ID
  getStudent(id: number): Observable<StudentDetailDto> {
    return this.http.get<StudentDetailDto>(`${this.apiUrl}/${id}`);
  }

  // Create new student
  createStudent(student: Student): Observable<Student> {
    return this.http.post<Student>(this.apiUrl, student);
  }

  // Update student
  updateStudent(id: number, student: Student): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, student);
  }

  // Delete student
  deleteStudent(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
