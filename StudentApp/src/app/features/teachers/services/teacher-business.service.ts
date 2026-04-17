import { Injectable } from '@angular/core';
import { Observable, tap, catchError, throwError, map } from 'rxjs';
import { TeacherApiService } from '../../../core/services/http';
import { TeacherStateService, StudentAuthStateService } from '../../../core/services/state';
import { Teacher, CreateTeacherDto, LoginDto, TeacherLoginResponse, TeacherActivateDto } from '../../../core/models';

/**
 * BUSINESS LOGIC LAYER - Teacher Business Service
 * Orchestrates authentication and teacher management
 * Contains business rules and validation logic
 */
@Injectable({
  providedIn: 'root'
})
export class TeacherBusinessService {
  constructor(
    private teacherApi: TeacherApiService,
    private teacherState: TeacherStateService,
    private studentAuthState: StudentAuthStateService
  ) { }

  /**
   * Authenticate a teacher
   * Business Logic: Login, update authentication state
   */
  login(credentials: LoginDto): Observable<TeacherLoginResponse> {
    this.teacherState.setLoading(true);

    return this.teacherApi.login(credentials).pipe(
      tap(response => {
        // Map teacherId → id to match the Angular Teacher interface
        const teacher: Teacher = {
          id: response.teacher.teacherId,
          idPassportNo: response.teacher.idPassportNo || '',
          firstName: response.teacher.firstName,
          lastName: response.teacher.lastName,
          email: response.teacher.email,
          phone: response.teacher.phone,
          subjectId: response.teacher.subjectId,
          subjectName: response.teacher.subjectName,
          createdAt: response.teacher.createdDate
        };
        this.studentAuthState.logout(); // Clear any active student session
        this.teacherState.setCurrentTeacher(teacher);
        this.teacherState.setToken(response.token); // Store JWT for subsequent API calls
        this.teacherState.setLoading(false);
      }),
      catchError(error => {
        const errorMessage = this.extractErrorMessage(error);
        this.teacherState.setError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Activate a teacher account (first login — teacher sets their own password)
   * Business Logic: Validate, activate account, store session
   */
  activate(dto: TeacherActivateDto): Observable<Teacher> {
    if (!this.isValidEmail(dto.email)) {
      const error = 'Invalid email format';
      this.teacherState.setError(error);
      return throwError(() => new Error(error));
    }

    if (!this.isValidPassword(dto.password)) {
      const error = 'Password must be at least 6 characters';
      this.teacherState.setError(error);
      return throwError(() => new Error(error));
    }

    this.teacherState.setLoading(true);

    return this.teacherApi.activate(dto).pipe(
      map(response => {
        const teacher: Teacher = {
          id: response.teacher.teacherId,
          idPassportNo: response.teacher.idPassportNo || '',
          firstName: response.teacher.firstName,
          lastName: response.teacher.lastName,
          email: response.teacher.email,
          phone: response.teacher.phone,
          subjectId: response.teacher.subjectId,
          subjectName: response.teacher.subjectName,
          createdAt: response.teacher.createdDate
        };
        this.studentAuthState.logout();
        this.teacherState.setCurrentTeacher(teacher);
        this.teacherState.setToken(response.token);
        this.teacherState.setLoading(false);
        return teacher;
      }),
      catchError(error => {
        const errorMessage = this.extractErrorMessage(error);
        this.teacherState.setError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Update an existing teacher profile (admin / edit-profile path)
   * Business Logic: Validate email, call update API
   */
  register(teacherData: CreateTeacherDto): Observable<Teacher> {
    if (!this.isValidEmail(teacherData.email)) {
      const error = 'Invalid email format';
      this.teacherState.setError(error);
      return throwError(() => new Error(error));
    }

    this.teacherState.setLoading(true);

    return this.teacherApi.create(teacherData).pipe(
      map(teacher => {
        this.teacherState.setCurrentTeacher(teacher);
        this.teacherState.setLoading(false);
        return teacher;
      }),
      catchError(error => {
        const errorMessage = this.extractErrorMessage(error);
        this.teacherState.setError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Logout current teacher
   * Business Logic: Clear authentication state
   */
  logout(): void {
    this.teacherState.logout();
  }

  /**
   * Load a single teacher by ID
   */
  loadTeacherById(id: number): Observable<Teacher> {
    this.teacherState.setLoading(true);

    return this.teacherApi.getById(id).pipe(
      tap(teacher => {
        this.teacherState.setCurrentTeacher(teacher);
        this.teacherState.setLoading(false);
      }),
      catchError(error => {
        const errorMessage = this.extractErrorMessage(error);
        this.teacherState.setError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Load all teachers (admin functionality)
   */
  loadTeachers(): Observable<Teacher[]> {
    this.teacherState.setLoading(true);
    
    return this.teacherApi.getAll().pipe(
      tap(teachers => {
        this.teacherState.setTeachers(teachers);
        this.teacherState.setLoading(false);
      }),
      catchError(error => {
        const errorMessage = this.extractErrorMessage(error);
        this.teacherState.setError(errorMessage);
        return throwError(() => error);
      })
    );
  }

  /**
   * Business Rule: Validate email format
   */
  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  /**
   * Business Rule: Validate password strength
   */
  private isValidPassword(password: string): boolean {
    return password.length >= 6;
  }

  /**
   * Extract user-friendly error message from HTTP error
   */
  private extractErrorMessage(error: any): string {
    if (error.error?.message) {
      return error.error.message;
    }
    if (error.message) {
      return error.message;
    }
    return 'An unexpected error occurred';
  }

  /**
   * Clear error from state
   */
  clearError(): void {
    this.teacherState.clearError();
  }
}
