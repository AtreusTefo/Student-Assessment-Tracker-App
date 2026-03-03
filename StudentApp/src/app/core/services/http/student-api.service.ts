import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  StudentListDto, 
  StudentDetailDto, 
  CreateStudentDto, 
  UpdateStudentDto 
} from '../../models';

/**
 * DATA ACCESS LAYER - Student HTTP API Service
 * Responsible ONLY for HTTP communication with backend API
 * No business logic, no state management
 * Pure data access operations
 */
@Injectable({
  providedIn: 'root'
})
export class StudentApiService {
  private readonly apiUrl = '/api/students';

  constructor(private http: HttpClient) { }

  /**
   * Fetch all students (list view)
   * @returns Observable of StudentListDto array
   */
  getAll(): Observable<StudentListDto[]> {
    return this.http.get<StudentListDto[]>(this.apiUrl);
  }

  /**
   * Fetch a single student by ID (detail view)
   * @param id - Student ID
   * @returns Observable of StudentDetailDto
   */
  getById(id: number): Observable<StudentDetailDto> {
    return this.http.get<StudentDetailDto>(`${this.apiUrl}/${id}`);
  }

  /**
   * Create a new student
   * @param student - Student data
   * @returns Observable of created StudentDetailDto
   */
  create(student: CreateStudentDto): Observable<StudentDetailDto> {
    return this.http.post<StudentDetailDto>(this.apiUrl, student);
  }

  /**
   * Update an existing student
   * @param id - Student ID
   * @param student - Updated student data
   * @returns Observable of void
   */
  update(id: number, student: UpdateStudentDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, student);
  }

  /**
   * Delete a student by ID
   * @param id - Student ID
   * @returns Observable of void
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
