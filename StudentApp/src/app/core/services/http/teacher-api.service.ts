import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Teacher, CreateTeacherDto, UpdateTeacherDto, LoginDto, TeacherLoginResponse, TeacherActivateDto } from '../../models';

/**
 * DATA ACCESS LAYER - Teacher HTTP API Service
 * Responsible ONLY for HTTP communication with backend API
 * No business logic, no state management
 * Pure data access operations
 */
@Injectable({
  providedIn: 'root'
})
export class TeacherApiService {
  private readonly apiUrl = '/api/teachers';

  constructor(private http: HttpClient) { }

  /**
   * Fetch all teachers
   * @returns Observable of Teacher array
   */
  getAll(): Observable<Teacher[]> {
    return this.http.get<Teacher[]>(this.apiUrl);
  }

  /**
   * Fetch a single teacher by ID
   * @param id - Teacher ID
   * @returns Observable of Teacher
   */
  getById(id: number): Observable<Teacher> {
    return this.http.get<Teacher>(`${this.apiUrl}/${id}`);
  }

  /**
   * Register a new teacher
   * @param teacher - Teacher registration data
   * @returns Observable of created Teacher
   */
  create(teacher: CreateTeacherDto): Observable<Teacher> {
    return this.http.post<Teacher>(this.apiUrl, teacher);
  }

  /**
   * Update an existing teacher
   * @param id - Teacher ID
   * @param teacher - Updated teacher data
   * @returns Observable of void
   */
  update(id: number, teacher: UpdateTeacherDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, teacher);
  }

  /**
   * Delete a teacher by ID
   * @param id - Teacher ID
   * @returns Observable of void
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Activate a teacher account (first-time login — teacher sets their own password)
   * POST /api/teachers/activate
   */
  activate(dto: TeacherActivateDto): Observable<TeacherLoginResponse> {
    return this.http.post<TeacherLoginResponse>(`${this.apiUrl}/activate`, dto);
  }

  /**
   * Authenticate a teacher
   * @param credentials - Login credentials
   * @returns Observable of authenticated Teacher
   */
  login(credentials: LoginDto): Observable<TeacherLoginResponse> {
    return this.http.post<TeacherLoginResponse>(`${this.apiUrl}/login`, credentials);
  }
}
