import { Injectable } from '@angular/core';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { TeacherApiService } from '../../../core/services/http';
import { TeacherStateService } from '../../../core/services/state';
import { Teacher, CreateTeacherDto, LoginDto } from '../../../core/models';

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
    private teacherState: TeacherStateService
  ) { }

  /**
   * Authenticate a teacher
   * Business Logic: Login, update authentication state
   */
  login(credentials: LoginDto): Observable<Teacher> {
    this.teacherState.setLoading(true);
    
    return this.teacherApi.login(credentials).pipe(
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
   * Register a new teacher
   * Business Logic: Validate, create teacher account
   */
  register(teacherData: CreateTeacherDto): Observable<Teacher> {
    // Business rule: Validate email format
    if (!this.isValidEmail(teacherData.email)) {
      const error = 'Invalid email format';
      this.teacherState.setError(error);
      return throwError(() => new Error(error));
    }

    // Business rule: Validate password strength
    if (!this.isValidPassword(teacherData.password)) {
      const error = 'Password must be at least 6 characters';
      this.teacherState.setError(error);
      return throwError(() => new Error(error));
    }

    this.teacherState.setLoading(true);
    
    return this.teacherApi.create(teacherData).pipe(
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
   * Logout current teacher
   * Business Logic: Clear authentication state
   */
  logout(): void {
    this.teacherState.logout();
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
